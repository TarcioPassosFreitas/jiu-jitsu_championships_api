using CommonUtility.Repository;
using JiuJitsuMaster.Core.Aggregates;
using JiuJitsuMaster.Core.Interfaces.Repositories;
using JiuJitsuMaster.Infrastructure.Database.Contexts;
using Microsoft.EntityFrameworkCore;

namespace JiuJitsuMaster.Infrastructure.Repositories;

public class AthleteChampionshipRepository : BaseRepository<AthleteChampionship>, IAthleteChampionshipRepository
{
    private readonly DatabaseContext _databaseContext;

    public AthleteChampionshipRepository(DatabaseContext dbContext)
        : base(dbContext)
    {
        _databaseContext = dbContext;
    }

    public async Task<AthleteChampionship?> GetByChampionshipAndAthleteAsync(long championshipId, long athleteId, CancellationToken cancellationToken)
    {
        return await _databaseContext.AthleteChampionships
            .Include(x => x.Athlete)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ChampionshipId == championshipId && x.AthleteId == athleteId, cancellationToken);
    }

    public async Task<List<AthleteChampionship>> GetByChampionshipAsync(long championshipId, CancellationToken cancellationToken)
    {
        return await _databaseContext.AthleteChampionships
            .Include(x => x.Athlete)
            .Where(x => x.ChampionshipId == championshipId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}