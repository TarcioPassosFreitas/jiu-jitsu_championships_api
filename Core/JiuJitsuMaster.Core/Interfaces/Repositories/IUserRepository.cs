using CommonUtility.Extensions;
using CommonUtility.Interfaces;
using JiuJitsuMaster.Core.Aggregates;

namespace JiuJitsuMaster.Core.Interfaces.Repositories;

public interface IUserRepository : IBaseRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken);

    Task<User?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken);

    Task UpdateRefreshTokenAsync(long id, string refreshToken, CancellationToken cancellationToken);

    Task ChangePasswordAsync(long id, string newPassword, CancellationToken cancellationToken);

    Task<PaginatedList<User>> ListAllAsync(int page, int limit, string? name, CancellationToken cancellationToken);
}