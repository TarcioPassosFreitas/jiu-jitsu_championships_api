using CommonUtility.Interfaces;
using JiuJitsuMaster.Core.Aggregates;
using System.Threading;

namespace JiuJitsuMaster.Core.Interfaces.Repositories;

public interface IAthleteFightRepository : IBaseRepository<AthleteFight>
{
    Task AddAllAsync(List<AthleteFight> athleteFights, CancellationToken cancellationToken);

    Task<bool> GetByRoundAsync(CancellationToken cancellationToken);

    Task<List<AthleteFight>> GetUnfinishedFightsAsync(CancellationToken cancellationToken);

    Task UpdateFightAsync(long id, bool isFinished, bool? winner, CancellationToken cancellationToken);

    Task<int> GetByNumberRoundAsync(CancellationToken cancellationToken);
}