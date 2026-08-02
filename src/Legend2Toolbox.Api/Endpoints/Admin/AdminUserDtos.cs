namespace Legend2Toolbox.Api.Endpoints.Admin;

public record ToggleLockRequest(bool LockUser);
public record AssignRoleRequest(string RoleName);
public record UpdateUserRequest(string Username, string Email);

public record GetUserByNameRequest(string UserName);
