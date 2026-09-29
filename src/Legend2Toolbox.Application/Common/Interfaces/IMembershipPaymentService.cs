using Legend2Toolbox.Domain.Enums;

namespace Legend2Toolbox.Application.Common.Interfaces;

public interface IMembershipPaymentService
{
    Task<Result<PaymentOrderDto>> CreateOrderAsync(Guid userId, PaymentProvider provider,
        CancellationToken cancellationToken = default);

    Task<Result<MembershipStatusDto>> HandleNotificationAsync(PaymentProvider provider,
        PaymentNotificationDto notification, CancellationToken cancellationToken = default);
}
