namespace Legend2Toolbox.Application.Common.Interfaces;

public interface IMembershipService
{
    Task<Result<MembershipStatusDto>> GrantRegistrationMembershipAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<MembershipStatusDto>> AdjustMembershipDaysAsync(string userId, int days, CancellationToken cancellationToken = default);
    Task<Result<MembershipStatusDto>> GrantPaidMembershipAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<MembershipStatusDto?> GetStatusAsync(Guid userId, CancellationToken cancellationToken = default);
    Task ExpireMembershipsAsync(CancellationToken cancellationToken = default);
}
