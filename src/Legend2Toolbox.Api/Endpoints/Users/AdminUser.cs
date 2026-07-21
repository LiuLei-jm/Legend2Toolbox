namespace Legend2Toolbox.Api.Endpoints.Users;

public record ToggleLockRequest(bool LockUser);
public record AssignRoleRequest(string RoleName);
public record UpdateUserRequest(string Username, string Email);
