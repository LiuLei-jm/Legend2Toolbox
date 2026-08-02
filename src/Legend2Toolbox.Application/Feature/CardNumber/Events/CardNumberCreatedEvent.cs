namespace Legend2Toolbox.Application.Feature.CardNumber.Events;

public record CardNumberCreatedEvent(
    Guid CardId,
    Guid UserId,
    string Cdk,
    string UserName
    ) : INotification;
