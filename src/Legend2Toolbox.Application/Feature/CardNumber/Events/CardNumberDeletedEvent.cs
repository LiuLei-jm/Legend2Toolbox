namespace Legend2Toolbox.Application.Feature.CardNumber.Events;

public record CardNumberDeletedEvent(
    Guid CardId,
    Guid UserId,
    string Cdk,
    string UserName) : INotification;
