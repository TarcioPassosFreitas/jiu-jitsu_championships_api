using Ardalis.Result;
using CommonUtility.Extensions;
using CommonUtility.Interfaces;
using JiuJitsuMaster.Core.Aggregates;

namespace JiuJitsuMaster.Core.Interfaces.Services;

public interface IUserService : IBaseService<User>
{
    Task<Result<User>> LoginAsync(string email, string password, CancellationToken cancellationToken);

    Task<Result<User>> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken);

    Task<Result<User>> GetByEmailAsync(string email, CancellationToken cancellationToken);

    Task<Result<User>> ResetPasswordAsync(long id, string newPassword, string confirmPassword, CancellationToken cancellationToken);

    Task<Result<User>> RegisterUsersAsync(User user, CancellationToken cancellationToken);

    Task<Result<User>> UpdateUserAsync(User user, CancellationToken cancellationToken);

    Task<Result<User>> ExcludeAsync(long id, CancellationToken cancellationToken);

    Task<Result<PaginatedList<User>>> ListAllAsync(int page, int limit, string? name, CancellationToken cancellationToken);

    Task<Result<Athlete>> ResetAthletePasswordAsync(long id, string newPassword, string confirmPassword, CancellationToken cancellationToken);
}