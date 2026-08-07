namespace Legend2Toolbox.Api.Endpoints.Identity;

public record RegisterRequest(string Username, string Email, string Password);

public record LoginRequest(string Username, string Password);

public record ChangePasswordRequest(string OldPassword, string NewPassword);

public record ForgotPasswordRequest(string Email, string ClientResetUrl);

public record ResetPasswordRequest(string Email, string Token, string NewPassword);

