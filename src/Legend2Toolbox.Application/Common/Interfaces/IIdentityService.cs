using Legend2Toolbox.Application.Feature.Identity;
using Legend2Toolbox.Domain.Models;
using System.Security.Claims;

namespace Legend2Toolbox.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<Result> RegisterUserAsync(RegisterCommand request);
    Task<Result> ChangePasswordAsync(ChangePasswordCommand request);
    Task<Result> ForgotPasswordAsync(ForgotPasswordCommand request);
    Task<Result> ResetPasswordAsync(ResetPasswordCommand request);
    Task<Result<ClaimsPrincipal>> AuthenticateUserAsync(LoginQuery request);
}
