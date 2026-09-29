using Legend2Toolbox.Domain.Enums;

namespace Legend2Toolbox.Application.Common.Models;

public record PaymentOrderDto(
    string OrderId,
    PaymentProvider Provider,
    decimal Amount,
    int DurationDays,
    string? CheckoutUrl,
    string Status);

public record PaymentNotificationDto(
    string OrderId,
    decimal Amount,
    string TradeNo,
    string Signature,
    string? Payload = null);
