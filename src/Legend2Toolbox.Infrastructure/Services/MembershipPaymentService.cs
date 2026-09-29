using Legend2Toolbox.Domain.Enums;

namespace Legend2Toolbox.Infrastructure.Services;

internal sealed class MembershipPaymentService : IMembershipPaymentService
{
    private const decimal MembershipPrice = 50.00m;
    private const int MembershipDays = 30;
    private readonly ApplicationDbContext _context;
    private readonly IReadOnlyDictionary<PaymentProvider, IPaymentGateway> _gateways;
    private readonly IMembershipService _membershipService;

    public MembershipPaymentService(ApplicationDbContext context, IEnumerable<IPaymentGateway> gateways,
        IMembershipService membershipService)
    {
        _context = context;
        _gateways = gateways.ToDictionary(x => x.Provider);
        _membershipService = membershipService;
    }

    public async Task<Result<PaymentOrderDto>> CreateOrderAsync(Guid userId, PaymentProvider provider,
        CancellationToken cancellationToken = default)
    {
        if (!_gateways.TryGetValue(provider, out var gateway))
            return Result<PaymentOrderDto>.Failure("不支持的支付方式");

        var order = MembershipPaymentOrder.Create(userId, provider, MembershipPrice, MembershipDays);
        _context.MembershipPaymentOrders.Add(order);
        await _context.SaveChangesAsync(cancellationToken);

        var checkoutResult = await gateway.CreateCheckoutUrlAsync(order, cancellationToken);
        if (checkoutResult.IsFailure)
        {
            order.MarkCancelled();
            await _context.SaveChangesAsync(cancellationToken);
            return Result<PaymentOrderDto>.Failure(checkoutResult.Errors);
        }

        return Result<PaymentOrderDto>.Success(ToDto(order, checkoutResult.Value));
    }

    public async Task<Result<MembershipStatusDto>> HandleNotificationAsync(PaymentProvider provider,
        PaymentNotificationDto notification, CancellationToken cancellationToken = default)
    {
        if (!_gateways.TryGetValue(provider, out var gateway) || !gateway.VerifyNotification(notification))
            return Result<MembershipStatusDto>.Failure("支付回调验签失败");
        if (notification.Amount != MembershipPrice)
            return Result<MembershipStatusDto>.Failure("支付金额必须为50元");

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        var order = await _context.MembershipPaymentOrders
            .SingleOrDefaultAsync(x => x.OrderId == notification.OrderId, cancellationToken);
        if (order is null || order.Provider != provider || order.Amount != notification.Amount)
            return Result<MembershipStatusDto>.Failure("支付订单信息校验失败");
        if (order.Status == PaymentOrderStatus.Cancelled)
            return Result<MembershipStatusDto>.Failure("支付订单已取消");

        if (order.Status == PaymentOrderStatus.Paid)
        {
            var existing = await _membershipService.GetStatusAsync(order.UserId, cancellationToken);
            if (existing is null) return Result<MembershipStatusDto>.Failure("会员状态不存在");
            await transaction.CommitAsync(cancellationToken);
            return Result<MembershipStatusDto>.Success(existing);
        }

        var membershipResult = await _membershipService.GrantPaidMembershipAsync(order.UserId,
            cancellationToken);
        if (membershipResult.IsFailure) return membershipResult;

        order.MarkPaid(notification.TradeNo);
        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return membershipResult;
    }

    private static PaymentOrderDto ToDto(MembershipPaymentOrder order, string checkoutUrl) =>
        new(order.OrderId, order.Provider, order.Amount, order.DurationDays, checkoutUrl,
            order.Status.ToString());
}
