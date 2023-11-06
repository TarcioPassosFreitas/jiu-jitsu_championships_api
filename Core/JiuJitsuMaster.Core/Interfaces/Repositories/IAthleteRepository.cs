using CommonUtility.Extensions;
using CommonUtility.Interfaces;
using JiuJitsuMaster.Core.Aggregates;
using JiuJitsuMaster.Core.Enums;

namespace JiuJitsuMaster.Core.Interfaces.Repositories;

public interface IAthleteRepository : IBaseRepository<Athlete>
{
    Task<Athlete?> GetByEmailAsync(string email, CancellationToken cancellationToken);

    Task<Athlete?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken);

    Task UpdateRefreshTokenAsync(long id, string refreshToken, CancellationToken cancellationToken);

    Task ChangePasswordAsync(long id, string newPassword, CancellationToken cancellationToken);

    Task<PaginatedList<Athlete>> GetAthleteRegistrationsAsync(
        int page, int pageSize, string? athleteName, Gender? gender, string? championshipTitle,
        string? city, string? state, CancellationToken cancellationToken);

    Task<List<Athlete>> GetAthleteRegistrationsAsync(
        string? athleteName, Gender? gender, string? championshipTitle,
        string? city, string? state, CancellationToken cancellationToken);
}