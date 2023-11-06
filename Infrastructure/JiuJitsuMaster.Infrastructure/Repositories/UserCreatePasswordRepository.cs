using CommonUtility.Repository;
using JiuJitsuMaster.Core.Aggregates;
using JiuJitsuMaster.Core.Interfaces.Repositories;
using JiuJitsuMaster.Infrastructure.Database.Contexts;
using Microsoft.EntityFrameworkCore;

namespace JiuJitsuMaster.Infrastructure.Repositories;

public class UserCreatePasswordRepository : BaseRepository<UserCreatePassword>, IUserCreatePasswordRepository
{
    private readonly DatabaseContext _databaseContext;

    public UserCreatePasswordRepository(DatabaseContext dbContext)
        : base(dbContext)
    {
        _databaseContext = dbContext;
    }

    public async Task ExpireToken(long id, DateTime expire, CancellationToken cancellationToken)
    {
        var userCreatePassword = new UserCreatePassword
        {
            Id = id,
            Expire = expire,
            UpdatedAt = DateTime.UtcNow
        };

        _databaseContext.Set<UserCreatePassword>().Attach(userCreatePassword);
        _databaseContext.Entry(userCreatePassword).Property(x => x.Expire).IsModified = true;
        _databaseContext.Entry(userCreatePassword).Property(x => x.UpdatedAt).IsModified = true;

        await _databaseContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<UserCreatePassword?> GetByTokenAsync(string token, CancellationToken cancellationToken)
    {
        return await _databaseContext.UserCreatePasswords.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Token == token, cancellationToken);
    }
}