using CommonUtility.Interfaces;
using JiuJitsuMaster.Core.Aggregates;

namespace JiuJitsuMaster.Core.Interfaces.Repositories;

public interface IUserCreatePasswordRepository : IBaseRepository<UserCreatePassword>
{
    Task ExpireToken(long id, DateTime expire, CancellationToken cancellationToken);
    Task<UserCreatePassword?> GetByTokenAsync(string token, CancellationToken cancellationToken);
}