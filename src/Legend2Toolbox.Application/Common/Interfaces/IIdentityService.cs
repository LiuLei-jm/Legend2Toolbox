
namespace Legend2Toolbox.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<Result> RegisterUserAsync(RegisterCommand request);
    Task<Result> ChangePasswordAsync(ChangePasswordCommand request);
    Task<Result> ForgotPasswordAsync(ForgotPasswordCommand request);
    Task<Result> ResetPasswordAsync(ResetPasswordCommand request);
    Task<Result<ClaimsPrincipal>> AuthenticateUserAsync(LoginCommand request);
    Task<Result<UserInfoDto>> GetUserInfoAsync(GetUserInfoQuery request);

    Task<Result<PagedResult<UserDto>>> GetAllUsersAsync(int pageNumber, int pageSize);
    Task<Result<UserDto>> GetUserByNameAsync(string name);
    Task<Result> AssignRoleAsync(AssignRoleCommand request);
    Task<Result> ToggleUserLockAsync(ToggleUserLockCommand request);
    Task<Result> UpdateUserAsync(UpdateUserCommand request);
    Task<Result> RemoveUserAsync(RemoveUserCommand request);
    Task<Result> DeleteUserAsync(string userId);
}
