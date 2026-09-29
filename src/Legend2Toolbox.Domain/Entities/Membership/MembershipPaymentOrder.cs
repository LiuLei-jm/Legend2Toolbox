using Legend2Toolbox.Domain.Enums;

namespace Legend2Toolbox.Domain.Entities.Membership;

public class MembershipPaymentOrder
{
    private MembershipPaymentOrder()
    {
    }

    public Guid Id { get; private set; }
    public string OrderId { get; private set; } = string.Empty;
    public Guid UserId { get; private set; }
    public PaymentProvider Provider { get; private set; }
    public decimal Amount { get; private set; }
    public int DurationDays { get; private set; }
    public PaymentOrderStatus Status { get; private set; }
    public string? TradeNo { get; private set; }
    public DateTimeOffset CreatedOn { get; private set; }
    public DateTimeOffset? PaidOn { get; private set; }

    public static MembershipPaymentOrder Create(Guid userId, PaymentProvider provider, decimal amount,
        int durationDays)
    {
        var now = DateTimeOffset.UtcNow;
        return new MembershipPaymentOrder
        {
            Id = Guid.NewGuid(),
            OrderId = $"MEM{now:yyyyMMddHHmmssfff}{Random.Shared.Next(100000, 999999)}",
            UserId = userId,
            Provider = provider,
            Amount = amount,
            DurationDays = durationDays,
            Status = PaymentOrderStatus.Pending,
            CreatedOn = now
        };
    }

    public void MarkPaid(string tradeNo)
    {
        if (Status == PaymentOrderStatus.Paid) return;
        Status = PaymentOrderStatus.Paid;
        TradeNo = tradeNo;
        PaidOn = DateTimeOffset.UtcNow;
    }

    public void MarkCancelled()
    {
        if (Status == PaymentOrderStatus.Pending) Status = PaymentOrderStatus.Cancelled;
    }
}

public enum PaymentOrderStatus
{
    Pending,
    Paid,
    Cancelled
}
