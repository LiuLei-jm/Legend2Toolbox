namespace Legend2Toolbox.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ICurrentUserService _currentUserService;
    private readonly IEmailSender _emailSender;
    public IdentityService(UserManager<ApplicationUser> userManager, ICurrentUserService currentUserService, IEmailSender emailSender)
    {
        _userManager = userManager;
        _currentUserService = currentUserService;
        _emailSender = emailSender;
    }
    public async Task<Result<ClaimsPrincipal>> AuthenticateUserAsync(LoginCommand request)
    {
        var user = await _userManager.FindByNameAsync(request.Username);
        if (user == null)
        {
            return Result<ClaimsPrincipal>.Failure(ErrorMessages.Auth.InvalidCredentials);
        }
        if (await _userManager.IsLockedOutAsync(user))
        {
            var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);
            var timeLeft = lockoutEnd.HasValue ? lockoutEnd.Value - DateTimeOffset.UtcNow : TimeSpan.Zero;
            return Result<ClaimsPrincipal>.Failure(string.Format(ErrorMessages.Auth.UserLockedOut,
                                                   Math.Ceiling(timeLeft.TotalMinutes)));
        }
        if (!await _userManager.CheckPasswordAsync(user, request.Password))
        {
            await _userManager.AccessFailedAsync(user);
            if (await _userManager.IsLockedOutAsync(user))
            {
                var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);
                var timeLeft = lockoutEnd.HasValue ? lockoutEnd.Value - DateTimeOffset.UtcNow : TimeSpan.Zero;
                return Result<ClaimsPrincipal>.Failure(string.Format(ErrorMessages.Auth.UserLockedOut, Math.Ceiling(timeLeft.TotalMinutes)));
            }
            return Result<ClaimsPrincipal>.Failure(ErrorMessages.Auth.InvalidCredentials);
        }
        await _userManager.ResetAccessFailedCountAsync(user);

        var identity = new ClaimsIdentity(IdentityConstants.BearerScheme);
        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
        identity.AddClaim(new Claim(ClaimTypes.Name, user.UserName ?? ""));

        var roles = await _userManager.GetRolesAsync(user);
        foreach (var role in roles)
        {
            identity.AddClaim(new Claim(ClaimTypes.Role, role));
        }
        return Result<ClaimsPrincipal>.Success(new ClaimsPrincipal(identity));
    }
    public async Task<Result> ChangePasswordAsync(ChangePasswordCommand request)
    {
        var userId = _currentUserService.UserId;
        if (userId == null) throw new UnauthorizedAccessException(ErrorMessages.Auth.TokenExpired);
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) throw new UnauthorizedAccessException(ErrorMessages.Auth.TokenExpired);
        var result = await _userManager.ChangePasswordAsync(user, request.OldPassword, request.NewPassword);
        if (result.Succeeded) return Result.Success();
        else
            return Result.Failure([.. result.Errors.Select(e => e.Description)]);
    }
    public async Task<Result> ForgotPasswordAsync(ForgotPasswordCommand request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null) return Result.Success();
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        var encodedToken = HttpUtility.UrlEncode(token);
        var encodedEmail = HttpUtility.UrlEncode(user.Email);
        var resetLink = $"{request.ClientResetUrl}?email={encodedEmail}&token={encodedToken}";
        var mailBody = $"<h3>您正在申请重置密码</h3>" +
            $"<p>请在24小时内点击下方链接完成重置: </p>" +
            $"<a href='{resetLink}' style='color:blue;'> 点击此处重置您的账户密码</a>" +
            $"<p>如果您并未发起过此申请，请忽略本邮件。</p>" +
            $"<p>{token}</p>";

        await _emailSender.SendEmailAsync(user.Email!, "【Legend工具箱】账户密码重置申请", mailBody);
        return Result.Success();
    }
    public async Task<Result> RegisterUserAsync(RegisterCommand request)
    {
        var existingEmailUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingEmailUser != null) return Result.Failure(ErrorMessages.Auth.EmailAlreadyExists);
        var user = new ApplicationUser
        {
            UserName = request.Username,
            Email = request.Email
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, nameof(Roles.Guest));
            return Result.Success();
        }
        else
            return Result.Failure(result.Errors.Select(e => e.Description).ToArray());
    }
    public async Task<Result> ResetPasswordAsync(ResetPasswordCommand request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return Result.Failure("无效的重置请求.");
        }
        var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        if (result.Succeeded)
        {
            await _userManager.ResetAccessFailedCountAsync(user);
            await _userManager.UpdateSecurityStampAsync(user);
            return Result.Success();
        }
        return Result.Failure(result.Errors.Select(e => e.Description).ToArray());
    }
    public async Task<Result> AssignRoleAsync(AssignRoleCommand request)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null) return Result.Failure("未找到该用户");

        if (!await _userManager.IsInRoleAsync(user, request.RoleName))
        {
            var result = await _userManager.AddToRoleAsync(user, request.RoleName);
            if (!result.Succeeded)
            {
                return Result.Failure(result.Errors.Select(e => e.Description).ToArray());
            }
        }
        return Result.Success();
    }
    public async Task<Result> ToggleUserLockAsync(ToggleUserLockCommand request)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null) return Result.Failure("未找到该用户");

        IdentityResult result;
        if (request.LockUser)
        {
            result = await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
        }
        else
        {
            result = await _userManager.SetLockoutEndDateAsync(user, null);
            await _userManager.ResetAccessFailedCountAsync(user);
        }

        if (!result.Succeeded)
        {
            return Result.Failure(result.Errors.Select(e => e.Description).ToArray());
        }
        return Result.Success();
    }
    public async Task<Result<PagedResult<UserDto>>> GetAllUsersAsync(int pageNumber, int pageSize)
    {
        var query = _userManager.Users.AsNoTracking();
        var totalCount = await query.CountAsync();
        var users = await query
            .OrderBy(u => u.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var userDtos = new List<UserDto>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var isLocked = await _userManager.IsLockedOutAsync(user);
            var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);

            userDtos.Add(new UserDto(
                user.Id.ToString(),
                user.UserName ?? "",
                user.Email ?? "",
                roles,
                isLocked,
                lockoutEnd
                ));
        }

        var pagedResult = new PagedResult<UserDto>(userDtos, pageNumber, pageSize, totalCount);
        return Result<PagedResult<UserDto>>.Success(pagedResult);
    }
    public async Task<Result> UpdateUserAsync(UpdateUserCommand request)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null) return Result.Failure("未找到该用户");

        var existingEmailUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingEmailUser != null && existingEmailUser.Id.ToString() != request.UserId)
        {
            return Result.Failure("该邮箱已被其他用户占用");
        }

        var existingNameUser = await _userManager.FindByNameAsync(request.Username);
        if (existingNameUser != null && existingNameUser.Id.ToString() != request.UserId)
        {
            return Result.Failure("该用户名已被其他用户占用");
        }

        if (user.UserName != AdminInfo.AdminUserName)
        {
            user.UserName = request.Username;
            user.NormalizedUserName = request.Username.ToUpperInvariant();
        }
        user.Email = request.Email;
        user.NormalizedEmail = request.Email.ToUpperInvariant();

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded) return Result.Failure(result.Errors.Select(e => e.Description).ToArray());

        return Result.Success();
    }
    public async Task<Result> DeleteUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return Result.Failure("未找到该用户");
        if (user.UserName == AdminInfo.AdminUserName) return Result.Failure("超级管理员无法删除");

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded) return Result.Failure(result.Errors.Select(e => e.Description).ToArray());
        return Result.Success();
    }

    public async Task<Result<UserDto>> GetUserByNameAsync(string name)
    {
        var user = await _userManager.FindByNameAsync(name);
        if (user == null) return Result<UserDto>.Failure("未找到该用户");
            var roles = await _userManager.GetRolesAsync(user);
            var isLocked = await _userManager.IsLockedOutAsync(user);
            var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);
        var userDto = new UserDto(
                user.Id.ToString(),
                user.UserName ?? "",
                user.Email ?? "",
                roles,
                isLocked,
                lockoutEnd
            );
        return Result<UserDto>.Success(userDto);
    }
}
