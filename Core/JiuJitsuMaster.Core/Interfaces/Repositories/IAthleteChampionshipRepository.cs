using CommonUtility.Interfaces;
using JiuJitsuMaster.Core.Aggregates;

namespace JiuJitsuMaster.Core.Interfaces.Repositories;

public interface IAthleteChampionshipRepository : IBaseRepository<AthleteChampionship>
{
    Task<List<AthleteChampionship>> GetByChampionshipAsync(long championshipId, CancellationToken cancellationToken);

    Task<AthleteChampionship?> GetByChampionshipAndAthleteAsync(long championshipId, long athleteId, CancellationToken cancellationToken);
}