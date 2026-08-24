
namespace Legend2Toolbox.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IEmailSender _emailSender;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;

    public IdentityService(UserManager<ApplicationUser> userManager, ICurrentUserService currentUserService,
        IEmailSender emailSender, ApplicationDbContext context,
         ITokenService tokenService)
    {
        _userManager = userManager;
        _currentUserService = currentUserService;
        _emailSender = emailSender;
        _context = context;
        _tokenService = tokenService;
    }

    public async Task<Result<AuthResponse>> LoginUserAsync(LoginCommand request)
    {
        var user = await _userManager.FindByNameAsync(request.Username);
        if (user == null || !user.IsActive || user.IsDeleted)
            return Result<AuthResponse>.Failure(ErrorMessages.Auth.InvalidCredentials);
        if (await _userManager.IsLockedOutAsync(user))
        {
            var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);
            var timeLeft = lockoutEnd.HasValue ? lockoutEnd.Value - DateTimeOffset.UtcNow : TimeSpan.Zero;
            return Result<AuthResponse>.Failure(string.Format(ErrorMessages.Auth.UserLockedOut,
                Math.Ceiling(timeLeft.TotalMinutes)));
        }

        if (!await _userManager.CheckPasswordAsync(user, request.Password))
        {
            await _userManager.AccessFailedAsync(user);
            if (await _userManager.IsLockedOutAsync(user))
            {
                var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);
                var timeLeft = lockoutEnd.HasValue ? lockoutEnd.Value - DateTimeOffset.UtcNow : TimeSpan.Zero;
                return Result<AuthResponse>.Failure(string.Format(ErrorMessages.Auth.UserLockedOut,
                    Math.Ceiling(timeLeft.TotalMinutes)));
            }

            return Result<AuthResponse>.Failure(ErrorMessages.Auth.InvalidCredentials);
        }

        await _userManager.ResetAccessFailedCountAsync(user);
        user.LastLoginAt = DateTimeOffset.UtcNow;

        var (accessToken, expiresAt) = await _tokenService.GenerateAccessTokenAsync(user);

        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        user.RefreshToken = refreshToken;

        user.RefreshTokenExpiryTime = DateTimeOffset.UtcNow.AddDays(7);

        await _userManager.UpdateAsync(user);

        return Result<AuthResponse>.Success(new AuthResponse(accessToken, refreshToken, expiresAt));
    }

    public async Task<Result> ChangePasswordAsync(ChangePasswordCommand request)
    {
        var userId = _currentUserService.UserId;
        if (userId == null) throw new UnauthorizedAccessException(ErrorMessages.Auth.TokenExpired);
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) throw new UnauthorizedAccessException(ErrorMessages.Auth.TokenExpired);
        var result = await _userManager.ChangePasswordAsync(user, request.OldPassword, request.NewPassword);
        if (result.Succeeded) return Result.Success();
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

        await _emailSender.SendEmailAsync(user.Email!, "【Legend2Toolbox】账户密码重置申请", mailBody);
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
            IsActive = true
        };
        user.ConnectionKey = ConnectionKey.Create(userId, user.UserName);
        user.CardNumberPath = CardNumberPath.Create(userId);

        var result = await _userManager.CreateAsync(user, request.Password);
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, nameof(Roles.Guest));
            return Result.Success();
        }

        return Result.Failure(result.Errors.Select(e => e.Description).ToArray());
    }

    public async Task<Result> ResetPasswordAsync(ResetPasswordCommand request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null) return Result.Failure(ErrorMessages.Auth.AccountNotExist);
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

        var currentRoles = await _userManager.GetRolesAsync(user);

        var removeRoles = currentRoles.Where(r => !request.RoleNames.Contains(r)).ToList();

        if (removeRoles.Count > 0)
        {
            var removeResult = await _userManager.RemoveFromRolesAsync(user, removeRoles);
            if (!removeResult.Succeeded)
            {
                return Result.Failure(removeResult.Errors.Select(e => e.Description).ToArray());
            }
        }

        var addRoles = request.RoleNames
            .Where(r => !currentRoles.Contains(r))
            .ToList();

        if(addRoles.Count > 0)
        {
            var addResult = await _userManager.AddToRolesAsync(user, addRoles);

            if (!addResult.Succeeded)
            {
                return Result.Failure(addResult.Errors.Select(e => e.Description).ToArray());
            }
        }

        return Result.Success();
    }

    public async Task<Result<bool>> ToggleUserLockAsync(ToggleUserLockCommand request)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null) return Result<bool>.Failure(ErrorMessages.Auth.AccountNotExist);
        if (user.UserName == AdminInfo.AdminUserName)
            return Result<bool>.Failure(ErrorMessages.Auth.CannotPerformedOnSuperAdmin);

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

        if (!result.Succeeded) return Result<bool>.Failure(result.Errors.Select(e => e.Description).ToArray());
        return Result<bool>.Success(request.LockUser);
    }

    public async Task<Result> UpdateUserProfileAsync(UpdateUserProfileCommand request)
    {
        var user = await _userManager.FindByIdAsync(_currentUserService.UserId!);
        if (user == null) return Result.Failure(ErrorMessages.Auth.AccountNotExist);
        user.NickName = request.NickName;
        user.Email = request.Email;
        user.PhoneNumber = request.PhoneNumber;
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded) return Result.Failure(result.Errors.Select(e => e.Description).ToArray());
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
            return Result<PagedResult<UserDto>>.Success(
                new PagedResult<UserDto>(new List<UserDto>(), pageNumber, pageSize, totalCount));

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
        if (user.UserName == AdminInfo.AdminUserName)
            return Result.Failure(ErrorMessages.Auth.CannotPerformedOnSuperAdmin);

        var existingEmailUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingEmailUser != null && existingEmailUser.Id.ToString() != request.UserId)
            return Result.Failure(ErrorMessages.Auth.EmailAlreadyExists);

        var existingNameUser = await _userManager.FindByNameAsync(request.Username);
        if (existingNameUser != null && existingNameUser.Id.ToString() != request.UserId)
            return Result.Failure(ErrorMessages.Auth.UsernameAlreadyExists);

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
        if (user.UserName == AdminInfo.AdminUserName)
            return Result.Failure(ErrorMessages.Auth.CannotPerformedOnSuperAdmin);
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
        if (user.UserName == AdminInfo.AdminUserName)
            return Result.Failure(ErrorMessages.Auth.CannotPerformedOnSuperAdmin);
        if (user.IsDeleted) return Result.Failure(ErrorMessages.Auth.AccountNotExist);
        user.IsDeleted = true;
        user.IsActive = false;
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded) return Result.Failure(string.Join(";", result.Errors.Select(e => e.Description)));
        return Result.Success();
    }

    public async Task<Result<UserInfoDto>> GetUserInfoAsync(GetUserInfoQuery request)
    {
        var user = await _userManager.FindByIdAsync(_currentUserService.UserId ?? "");
        if (user is null) return Result<UserInfoDto>.Failure(ErrorMessages.Auth.AccountNotExist);
        var roles = await _userManager.GetRolesAsync(user);
        var userInfo = new UserInfoDto
        {
            UserId = user.Id.ToString(),
            UserName = user.UserName ?? "",
            NickName = user.NickName,
            Email = user.Email ?? "",
            PhoneNumber = user.PhoneNumber ?? "",
            LastLoginAt = user.LastLoginAt ?? DateTimeOffset.UtcNow,
            Roles = roles
        };
        return Result<UserInfoDto>.Success(userInfo);
    }

    public async Task<Result<AuthResponse>> RefreshTokenAsync(RefreshTokenCommand request)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return Result<AuthResponse>.Failure(ErrorMessages.Auth.TokenExpired);
        }

        var user = await _userManager.Users
            .FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken);

        if (user == null)
        {
            return Result<AuthResponse>.Failure(ErrorMessages.Auth.RefreshDenied);
        }

        if (!user.IsActive || user.IsDeleted || await _userManager.IsLockedOutAsync(user))
        {
            return Result<AuthResponse>.Failure(ErrorMessages.Auth.RefreshDenied);
        }

        if (user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            return Result<AuthResponse>.Failure(ErrorMessages.Auth.TokenExpired);
        }

        var (newAccessToken, expiresAt) = await _tokenService.GenerateAccessTokenAsync(user);
        var newRefreshToken = _tokenService.GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _userManager.UpdateAsync(user);

        return Result<AuthResponse>.Success(new AuthResponse(newAccessToken, newRefreshToken, expiresAt));
    }
}