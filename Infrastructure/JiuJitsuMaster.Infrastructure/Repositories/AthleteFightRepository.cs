using CommonUtility.Repository;
using JiuJitsuMaster.Core.Aggregates;
using JiuJitsuMaster.Core.Interfaces.Repositories;
using JiuJitsuMaster.Infrastructure.Database.Contexts;
using Microsoft.EntityFrameworkCore;

namespace JiuJitsuMaster.Infrastructure.Repositories;

public class AthleteFightRepository : BaseRepository<AthleteFight>, IAthleteFightRepository
{
    private readonly DatabaseContext _databaseContext;

    public AthleteFightRepository(DatabaseContext dbContext)
        : base(dbContext)
    {
        _databaseContext = dbContext;
    }

    public async Task AddAllAsync(List<AthleteFight> athleteFights, CancellationToken cancellationToken)
    {
        await _databaseContext.AthleteFights
            .AddRangeAsync(athleteFights, cancellationToken);

        await _databaseContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> GetByNumberRoundAsync(CancellationToken cancellationToken)
    {
        var maxRound = await _databaseContext.AthleteFights
            .AsNoTracking()
            .Where(f => f.Fight != null)
            .Select(f => f.Fight.Round)
            .MaxAsync(cancellationToken);

        return maxRound;
    }


    public async Task<bool> GetByRoundAsync(CancellationToken cancellationToken)
    {
        var currentRound = await _databaseContext.Fights
            .AsNoTracking()
            .MaxAsync(f => f.Round, cancellationToken);

        var unfinishedFightsCount = await _databaseContext.AthleteFights
            .AsNoTracking()
            .Include(af => af.Fight)
            .Where(af => af.Fight.Round == currentRound && !af.IsFinished)
            .CountAsync(cancellationToken);

        return unfinishedFightsCount == 0;
    }

    public async Task<List<AthleteFight>> GetUnfinishedFightsAsync(CancellationToken cancellationToken)
    {
        return await _databaseContext.AthleteFights
            .Where(af => !af.IsFinished)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateFightAsync(long id, bool isFinished, bool? winner, CancellationToken cancellationToken)
    {
        var fight = new AthleteFight
        {
            Id = id,
            IsFinished = isFinished,
            Winner = winner,
            UpdatedAt = DateTime.UtcNow
        };
        _databaseContext.Set<AthleteFight>().Attach(fight);
        _databaseContext.Entry(fight).Property(x => x.IsFinished).IsModified = true;
        _databaseContext.Entry(fight).Property(x => x.Winner).IsModified = true;
        _databaseContext.Entry(fight).Property(x => x.UpdatedAt).IsModified = true;

        await _databaseContext.SaveChangesAsync(cancellationToken);
    }

}