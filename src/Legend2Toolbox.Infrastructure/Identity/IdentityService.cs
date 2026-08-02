namespace Legend2Toolbox.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ICurrentUserService _currentUserService;
    private readonly IEmailSender _emailSender;
    private readonly ApplicationDbContext _context;
    public IdentityService(UserManager<ApplicationUser> userManager, ICurrentUserService currentUserService, IEmailSender emailSender, ApplicationDbContext context)
    {
        _userManager = userManager;
        _currentUserService = currentUserService;
        _emailSender = emailSender;
        _context = context;
    }
    public async Task<Result<ClaimsPrincipal>> AuthenticateUserAsync(LoginCommand request)
    {
        var user = await _userManager.FindByNameAsync(request.Username);
        if (user == null)
        {
            return Result<ClaimsPrincipal>.Failure(ErrorMessages.Auth.InvalidCredentials);
        }
        if (!user.IsActive || user.IsDeleted) return Result<ClaimsPrincipal>.Failure(ErrorMessages.Auth.InvalidCredentials);
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
        user.LastLoginAt = DateTimeOffset.UtcNow;
        await _userManager.UpdateAsync(user);
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
        var userId = Guid.NewGuid();
        var user = new ApplicationUser
        {
            Id = userId,
            UserName = request.Username,
            Email = request.Email,
            IsActive = true,
        };
        user.SecurityKey = SecurityKey.Create(userId, user.UserName);
        user.CardNumberPath = CardNumberPath.Create(userId);

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
            return Result.Failure(ErrorMessages.Auth.AccountNotExist);
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
        if (user == null) return Result.Failure(ErrorMessages.Auth.AccountNotExist);

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
        if (user == null) return Result.Failure(ErrorMessages.Auth.AccountNotExist);
        if (user.UserName == AdminInfo.AdminUserName) return Result.Failure(ErrorMessages.Auth.CannotPerformedOnSuperAdmin);

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
        var query = _userManager.Users
                                      .AsNoTracking();
        var totalCount = await query.CountAsync();
        var users = await query
            .OrderBy(u => u.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        if (!users.Any())
        {
            return Result<PagedResult<UserDto>>.Success(
                new PagedResult<UserDto>(new List<UserDto>(), pageNumber, pageSize, totalCount));
        }

        var userIds = users.Select(u => u.Id).ToList();

        var userRolesQuery = await _context.UserRoles
            .Where(ur => userIds.Contains(ur.UserId))
            .Join(_context.Roles,
            ur => ur.RoleId,
            r => r.Id,
            (ur, r) => new { ur.UserId, RoleName = r.Name }).ToListAsync();

        var userRolesMap = userRolesQuery
            .GroupBy(x => x.UserId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.RoleName ?? "").ToList());


        var userDtos = new List<UserDto>();
        foreach (var user in users)
        {
            var lockoutEnd = user.LockoutEnd;
            var isLocked = user.LockoutEnabled && lockoutEnd.HasValue && lockoutEnd.Value > DateTimeOffset.UtcNow;
            var roles = userRolesMap.TryGetValue(user.Id, out var roleList)
                ? roleList
                : new List<string>();

            userDtos.Add(new UserDto(
                user.Id.ToString(),
                user.UserName ?? "",
                user.Email ?? "",
                roles,
                isLocked,
                lockoutEnd,
                user.IsActive
                ));
        }

        var pagedResult = new PagedResult<UserDto>(userDtos, pageNumber, pageSize, totalCount);
        return Result<PagedResult<UserDto>>.Success(pagedResult);
    }
    public async Task<Result> UpdateUserAsync(UpdateUserCommand request)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null) return Result.Failure(ErrorMessages.Auth.AccountNotExist);
        if (user.UserName == AdminInfo.AdminUserName) return Result.Failure(ErrorMessages.Auth.CannotPerformedOnSuperAdmin);

        var existingEmailUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingEmailUser != null && existingEmailUser.Id.ToString() != request.UserId)
        {
            return Result.Failure(ErrorMessages.Auth.EmailAlreadyExists);
        }

        var existingNameUser = await _userManager.FindByNameAsync(request.Username);
        if (existingNameUser != null && existingNameUser.Id.ToString() != request.UserId)
        {
            return Result.Failure(ErrorMessages.Auth.UsernameAlreadyExists);
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
        if (user == null) return Result.Failure(ErrorMessages.Auth.AccountNotExist);
        if (user.UserName == AdminInfo.AdminUserName) return Result.Failure(ErrorMessages.Auth.CannotPerformedOnSuperAdmin);
        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded) return Result.Failure(result.Errors.Select(e => e.Description).ToArray());
        return Result.Success();
    }
    public async Task<Result<UserDto>> GetUserByNameAsync(string name)
    {
        var user = await _userManager.FindByNameAsync(name);
        if (user == null) return Result<UserDto>.Failure(ErrorMessages.Auth.AccountNotExist);
        var roles = await _userManager.GetRolesAsync(user);
        var isLocked = await _userManager.IsLockedOutAsync(user);
        var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);
        var userDto = new UserDto(
                user.Id.ToString(),
                user.UserName ?? "",
                user.Email ?? "",
                roles,
                isLocked,
                lockoutEnd,
                user.IsActive
            );
        return Result<UserDto>.Success(userDto);
    }
    public async Task<Result> RemoveUserAsync(RemoveUserCommand request)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null) return Result.Failure(ErrorMessages.Auth.AccountNotExist);
        if (user.UserName == AdminInfo.AdminUserName) return Result.Failure(ErrorMessages.Auth.CannotPerformedOnSuperAdmin);
        if (user.IsDeleted) return Result.Failure(ErrorMessages.Auth.AccountNotExist);
        user.IsDeleted = true;
        user.IsActive = false;
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded) return Result.Failure(string.Join(";", result.Errors.Select(e => e.Description)));
        return Result.Success();
    }
}
