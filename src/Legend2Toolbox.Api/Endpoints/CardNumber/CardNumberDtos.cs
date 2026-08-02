namespace Legend2Toolbox.Api.Endpoints.CardNumber;

public record CreateCardNumberRequest(
    string Owner,
    int DurationInDays,
    double FaceValue,
    decimal Amount,
    DateTimeOffset StartTime,
    string? Notes);
public record UpdateCardNumberRequest(
    string Owner,
    int DurationInDays,
    double FaceValue,
    decimal Amount,
    DateTimeOffset StartTime,
    string? Notes);
public record UpdateCardNumberPathRequest(
    string BasePath,
    string FileName,
    bool AllowCustomPath);

