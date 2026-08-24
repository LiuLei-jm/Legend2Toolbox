namespace Legend2Toolbox.Application.Common.Models;

public record UserDto(
    string Id,
    string Username,
    string Email,
    IList<string> Roles,
    bool IsLockedOut,
    DateTimeOffset? LockoutEnd,
    bool IsActive);