namespace Legend2Toolbox.Api.Exceptions;

public class CustomIdentityErrorDescriber : IdentityErrorDescriber
{
    public override IdentityError DefaultError()
    {
        return new IdentityError { Code = nameof(DefaultError), Description = "发生一个未知错误." };
    }
    public override IdentityError ConcurrencyFailure()
    {
        return new IdentityError { Code = nameof(ConcurrencyFailure), Description = "并发冲突，对象已被修改." };
    }
    public override IdentityError PasswordRequiresNonAlphanumeric()
    {
        return new IdentityError { Code = nameof(PasswordRequiresNonAlphanumeric), Description = "密码必须至少包含一个非字母数字字符(如符号)." };
    }
    public override IdentityError PasswordRequiresDigit()
    {
        return new IdentityError { Code = nameof(PasswordRequiresDigit), Description = "密码必须至少包含一个数字." };
    }
    public override IdentityError PasswordRequiresLower()
    {
        return new IdentityError{Code= nameof(PasswordRequiresLower),Description = "密码必须至少包含一个小写字母."};
    }
    public override IdentityError PasswordRequiresUpper()
    {
        return new IdentityError { Code = nameof(PasswordRequiresUpper), Description = "密码必须至少包含一个大写字母." };
    }
    public override IdentityError DuplicateUserName(string userName)
    {
        return new IdentityError { Code = nameof(DuplicateUserName),Description = $"用户名 '{userName}' 已经被占用." };
    }
    public override IdentityError DuplicateEmail(string email)
    {
        return new IdentityError { Code = nameof(DuplicateEmail),Description = $"邮箱 '{email}' 已经被占用." };
    }
    public override IdentityError InvalidUserName(string? userName)
    {
        return new IdentityError { Code = nameof(InvalidUserName),Description = $"用户名 '{userName}' 不符合要求, 只能包含字母或数字." };
    }
    public override IdentityError InvalidEmail(string? email)
    {
        return new IdentityError { Code = nameof(InvalidEmail),Description = $"邮箱 '{email}' 不符合要求." };
    }
}
