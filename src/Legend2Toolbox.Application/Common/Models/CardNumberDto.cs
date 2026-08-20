namespace Legend2Toolbox.Application.Common.Models;

public record CardNumberDto(
    Guid Id,
    string Owner,
    int DurationInDays,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    double FaceValue,
    decimal Amount,
    string Cdk,
    bool IsExpired
);