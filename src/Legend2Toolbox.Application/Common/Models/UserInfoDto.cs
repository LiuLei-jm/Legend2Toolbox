namespace Legend2Toolbox.Application.Common.Models;

public class UserInfoDto
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string NickName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTimeOffset LastLoginAt { get; set; }
    public IList<string> Roles { get; set; } = [];
}