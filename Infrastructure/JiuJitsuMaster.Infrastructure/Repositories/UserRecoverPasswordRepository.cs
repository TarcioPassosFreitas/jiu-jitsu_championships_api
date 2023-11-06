using CommonUtility.Repository;
using JiuJitsuMaster.Core.Aggregates;
using JiuJitsuMaster.Core.Interfaces.Repositories;
using JiuJitsuMaster.Infrastructure.Database.Contexts;
using Microsoft.EntityFrameworkCore;

namespace JiuJitsuMaster.Infrastructure.Repositories;

public class UserRecoverPasswordRepository : BaseRepository<UserRecoverPassword>, IUserRecoverPasswordRepository
{
    private readonly DatabaseContext _databaseContext;

    public UserRecoverPasswordRepository(DatabaseContext dbContext)
        : base(dbContext)
    {
        _databaseContext = dbContext;
    }

    public async Task ExpireToken(long id, DateTime expire, CancellationToken cancellationToken)
    {
        var userExpiredRecoverPassword = new UserRecoverPassword
        {
            Id = id,
            Expire = expire,
            UpdatedAt = DateTime.UtcNow
        };

        _databaseContext.Set<UserRecoverPassword>().Attach(userExpiredRecoverPassword);
        _databaseContext.Entry(userExpiredRecoverPassword).Property(x => x.Expire).IsModified = true;
        _databaseContext.Entry(userExpiredRecoverPassword).Property(x => x.UpdatedAt).IsModified = true;

        await _databaseContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<UserRecoverPassword?> GetByTokenAsync(string token, CancellationToken cancellationToken)
    {
        return await _databaseContext.UserRecoverPasswords.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Token == token, cancellationToken);
    }
}
