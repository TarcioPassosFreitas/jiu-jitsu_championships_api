using CommonUtility.Interfaces;
using JiuJitsuMaster.Core.Aggregates;

namespace JiuJitsuMaster.Core.Interfaces.Repositories;

public interface IFightRepository : IBaseRepository<Fight>
{
    Task AddAllAsync(List<Fight> fights, CancellationToken cancellationToken);

    Task<List<Fight>> GetFightsByChampionshipId(long championshipId, CancellationToken cancellationToken);
}