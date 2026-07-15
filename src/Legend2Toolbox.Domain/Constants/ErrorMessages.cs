namespace Legend2Toolbox.Domain.Constants;

public static class ErrorMessages
{
    public static class Auth
    {
        public const string InvalidCredentials = "用户名或密码错误.";
        public const string UserLockedOut = "该账号因多次输错密码已被锁定，请在 {0} 分钟后重试.";
        public const string TokenExpired = "登录已过期，请重新登录.";
        public const string EmailAlreadyExists = "该邮箱已被注册.";
    }
}
