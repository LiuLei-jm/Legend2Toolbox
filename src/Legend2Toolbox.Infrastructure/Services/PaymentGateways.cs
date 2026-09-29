using Legend2Toolbox.Domain.Enums;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;

namespace Legend2Toolbox.Infrastructure.Services;

public class PaymentGatewayOptions
{
    public string CheckoutUrl { get; set; } = string.Empty;
    public string NotifyUrl { get; set; } = string.Empty;
    public string CallbackSecret { get; set; } = string.Empty;
}

public sealed class AlipayGatewayOptions : PaymentGatewayOptions
{
}

public sealed class WechatGatewayOptions : PaymentGatewayOptions
{
}

internal interface IPaymentGateway
{
    PaymentProvider Provider { get; }
    Task<Result<string>> CreateCheckoutUrlAsync(MembershipPaymentOrder order, CancellationToken cancellationToken);
    bool VerifyNotification(PaymentNotificationDto notification);
}

internal abstract class HmacPaymentGateway : IPaymentGateway
{
    private readonly PaymentGatewayOptions _options;

    protected HmacPaymentGateway(PaymentGatewayOptions options)
    {
        _options = options;
    }

    public abstract PaymentProvider Provider { get; }

    public Task<Result<string>> CreateCheckoutUrlAsync(MembershipPaymentOrder order, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.CheckoutUrl))
            return Task.FromResult(Result<string>.Failure($"未配置{Provider}支付下单地址"));

        var separator = _options.CheckoutUrl.Contains('?') ? "&" : "?";
        var url = $"{_options.CheckoutUrl}{separator}orderId={Uri.EscapeDataString(order.OrderId)}" +
                  $"&amount={order.Amount:F2}&durationDays={order.DurationDays}" +
                  (string.IsNullOrWhiteSpace(_options.NotifyUrl)
                      ? string.Empty
                      : $"&notifyUrl={Uri.EscapeDataString(_options.NotifyUrl)}");
        return Task.FromResult(Result<string>.Success(url));
    }

    public bool VerifyNotification(PaymentNotificationDto notification)
    {
        if (string.IsNullOrWhiteSpace(_options.CallbackSecret) ||
            string.IsNullOrWhiteSpace(notification.Signature)) return false;

        var content = $"{Provider}|{notification.OrderId}|{notification.Amount:F2}|{notification.TradeNo}";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_options.CallbackSecret));
        var expected = Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(content)));
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(expected),
            Encoding.UTF8.GetBytes(notification.Signature.Trim().ToUpperInvariant()));
    }
}

internal sealed class AlipayGateway : HmacPaymentGateway
{
    public AlipayGateway(IOptions<AlipayGatewayOptions> options) : base(options.Value)
    {
    }

    public override PaymentProvider Provider => PaymentProvider.Alipay;
}

internal sealed class WechatGateway : HmacPaymentGateway
{
    public WechatGateway(IOptions<WechatGatewayOptions> options) : base(options.Value)
    {
    }

    public override PaymentProvider Provider => PaymentProvider.Wechat;
}
