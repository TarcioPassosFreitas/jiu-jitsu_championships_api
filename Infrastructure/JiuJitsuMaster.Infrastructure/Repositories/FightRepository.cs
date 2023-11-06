using CommonUtility.Extensions;
using CommonUtility.Interfaces;
using CommonUtility.Repository;
using JiuJitsuMaster.Core.Aggregates;
using JiuJitsuMaster.Core.Interfaces.Repositories;
using JiuJitsuMaster.Infrastructure.Database.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JiuJitsuMaster.Infrastructure.Repositories;

public class FightRepository : BaseRepository<Fight>, IFightRepository
{
    private readonly DatabaseContext _databaseContext;
    private readonly ILogger<FightRepository> _logger;

    public FightRepository(DatabaseContext dbContext, ILogger<FightRepository> logger)
        : base(dbContext)
    {
        _databaseContext = dbContext;
        _logger = logger;
    }

    public async Task AddAllAsync(List<Fight> fights, CancellationToken cancellationToken)
    {
        await _databaseContext.Fights.AddRangeAsync(fights, cancellationToken);
        try
        {
            await _databaseContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Um erro ocorreu ao salvar as lutas no banco de dados.");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Um erro inesperado ocorreu.");
            throw;
        }
    }

    public async Task<List<Fight>> GetFightsByChampionshipId(long championshipId, CancellationToken cancellationToken)
    {
        return await _databaseContext.Fights
            .Where(f => f.ChampionshipId == championshipId && !f.Deleted)
            .Include(f => f.AthleteFights)
            .ThenInclude(af => af.Athlete)
            .OrderBy(f => f.Belt).ThenBy(f => f.Weight).ThenBy(f => f.Round)
            .ToListAsync(cancellationToken);
    }
}