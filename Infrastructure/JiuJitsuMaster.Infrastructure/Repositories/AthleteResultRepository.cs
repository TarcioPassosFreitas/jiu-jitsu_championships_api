using CommonUtility.Repository;
using JiuJitsuMaster.Core.Aggregates;
using JiuJitsuMaster.Core.Interfaces.Repositories;
using JiuJitsuMaster.Infrastructure.Database.Contexts;

namespace JiuJitsuMaster.Infrastructure.Repositories;

public class AthleteResultRepository : BaseRepository<AthleteResult>, IAthleteResultRepository
{
    private readonly DatabaseContext _databaseContext;

    public AthleteResultRepository(DatabaseContext dbContext)
        : base(dbContext)
    {
        _databaseContext = dbContext;
    }
}