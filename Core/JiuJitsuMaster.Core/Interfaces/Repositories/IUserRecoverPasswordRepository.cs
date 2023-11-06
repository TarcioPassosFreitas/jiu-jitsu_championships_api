using CommonUtility.Interfaces;
using JiuJitsuMaster.Core.Aggregates;

namespace JiuJitsuMaster.Core.Interfaces.Repositories;

public interface IUserRecoverPasswordRepository : IBaseRepository<UserRecoverPassword>
{
    Task ExpireToken(long id, DateTime expire, CancellationToken cancellationToken);
    Task<UserRecoverPassword?> GetByTokenAsync(string token, CancellationToken cancellationToken);
}