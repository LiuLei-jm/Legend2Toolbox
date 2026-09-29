namespace Legend2Toolbox.Application.Common.Models;

public record MembershipStatusDto(bool IsActive, DateTimeOffset? StartTime, DateTimeOffset? ExpireTime);
