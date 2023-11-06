using CommonUtility.Repository;
using JiuJitsuMaster.Core.Aggregates;
using JiuJitsuMaster.Core.Interfaces.Repositories;
using JiuJitsuMaster.Infrastructure.Database.Contexts;

namespace JiuJitsuMaster.Infrastructure.Repositories;

public class ResultRepository : BaseRepository<Results>, IResultRepository
{
    private readonly DatabaseContext _databaseContext;

    public ResultRepository(DatabaseContext dbContext)
        : base(dbContext)
    {
        _databaseContext = dbContext;
    }
}