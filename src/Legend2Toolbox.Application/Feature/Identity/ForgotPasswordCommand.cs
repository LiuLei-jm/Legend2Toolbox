namespace Legend2Toolbox.Application.Feature.Identity;

public record ForgotPasswordCommand(string Email, string ClientResetUrl) : IRequest<Result>;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Result>
{
    private readonly IIdentityService _identityService;

    public ForgotPasswordCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<Result> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        return await _identityService.ForgotPasswordAsync(request);
    }
}