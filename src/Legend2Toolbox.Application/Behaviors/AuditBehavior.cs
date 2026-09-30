using Legend2Toolbox.Application.Feature.CardNumber;
using Legend2Toolbox.Application.Feature.ConnectionKey;
using Legend2Toolbox.Application.Feature.Scripts.MaterialFile;
using Legend2Toolbox.Application.Feature.Scripts.ScriptFile;
using Legend2Toolbox.Application.Feature.Scripts.ScriptSet;
using Legend2Toolbox.Application.Feature.Scripts.ScriptSetDbData;

namespace Legend2Toolbox.Application.Behaviors;

public sealed class AuditBehavior<TRequest, TResponse>(IAuditService audit)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var operation = AuditOperations.Describe(request);
        return operation is null ? next() : audit.ExecuteAsync(operation, () => next());
    }
}

public static class AuditOperations
{
    // Deliberately explicit: some Commands are queries and some queries save internal metadata.
    public static AuditOperation? Describe(object request) => request switch
    {
        LoginCommand r => new("Auth", "Login", TargetName: r.Username, ActorType: "LoginAttempt"),
        RegisterCommand r => new("User", "Create", TargetName: r.Username, ActorType: "Registration"),
        ChangePasswordCommand => new("User", "ChangePassword"),
        ResetPasswordCommand => new("User", "ResetPassword", ActorType: "PasswordReset"),
        UpdateUserProfileCommand => new("User", "UpdateProfile"),
        UpdateUserCommand r => new("User", "Update", r.UserId),
        DeleteUserCommand r => new("User", "Delete", r.UserId),
        RemoveUserCommand r => new("User", "SoftDelete", r.UserId),
        AssignRoleCommand r => new("User", "AssignRoles", r.UserId),
        ToggleUserLockCommand r => new("User", r.LockUser ? "Lock" : "Unlock", r.UserId),
        AdjustMembershipDaysCommand r => new("Membership", "AdjustDays", r.UserId),
        CreateCardNumberCommand => new("CardNumber", "Create"),
        UpdateCardNumberCommand r => new("CardNumber", "Update", r.CardId.ToString()),
        DeleteCardNumberCommand r => new("CardNumber", "SoftDelete", r.CardId.ToString()),
        ReissueCardCommand r => new("CardNumber", "Reissue", r.CardId),
        CleanUpCardCommand r => new("CardNumber", "Cleanup", r.CardId),
        UpdateCardNumberPathCommand => new("CardNumberPath", "Update"),
        GenerateKeyCommand => new("ConnectionKey", "Rotate"),
        CreateScriptSetCommand => new("ScriptSet", "Create"),
        UpdateScriptSetCommand r => new("ScriptSet", "Update", r.Id.ToString()),
        DeleteScriptSetCommand r => new("ScriptSet", "Delete", r.Id.ToString()),
        CreateScriptFileCommand => new("ScriptFile", "Create"),
        UpdateScriptFileCommand r => new("ScriptFile", "Update", r.Id.ToString()),
        DeleteScriptFileCommand r => new("ScriptFile", "Delete", r.Id.ToString()),
        CreateMaterialFileCommand => new("MaterialFile", "Create"),
        UpdateMaterialFileCommand r => new("MaterialFile", "Update", r.Id.ToString()),
        DeleteMaterialFileCommand r => new("MaterialFile", "Delete", r.Id.ToString()),
        CreateScriptSetDbDataCommand => new("ScriptSetDbData", "Create"),
        UpdateScriptSetDbDataCommand r => new("ScriptSetDbData", "Update", r.Id.ToString()),
        DeleteScriptSetDbDataCommand r => new("ScriptSetDbData", "Delete", r.Id.ToString()),
        _ => null
    };
}
