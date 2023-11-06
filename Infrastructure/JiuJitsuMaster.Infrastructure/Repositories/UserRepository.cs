using CommonUtility.Extensions;
using CommonUtility.Repository;
using JiuJitsuMaster.Core.Aggregates;
using JiuJitsuMaster.Core.Interfaces.Repositories;
using JiuJitsuMaster.Infrastructure.Database.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace JiuJitsuMaster.Infrastructure.Repositories;

public class UserRepository : BaseRepository<User>, IUserRepository
{
    private readonly DatabaseContext _databaseContext;

    public UserRepository(DatabaseContext dbContext)
        : base(dbContext)
    {
        _databaseContext = dbContext;
    }

    public async Task ChangePasswordAsync(long id, string newPassword, CancellationToken cancellationToken)
    {
        var user = new User
        {
            Id = id,
            Password = newPassword,
            UpdatedAt = DateTime.UtcNow
        };

        _databaseContext.Set<User>().Attach(user);
        _databaseContext.Entry(user).Property(x => x.Password).IsModified = true;
        _databaseContext.Entry(user).Property(x => x.UpdatedAt).IsModified = true;

        await _databaseContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return await _databaseContext.Users.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Email.Equals(email), cancellationToken);
    }

    public async Task<User?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
    {
        return await _databaseContext.Users.AsNoTracking()
            .FirstOrDefaultAsync(x => x.RefreshToken == refreshToken, cancellationToken);
    }

    public async Task UpdateRefreshTokenAsync(long id, string refreshToken, CancellationToken cancellationToken)
    {
        var user = new User
        {
            Id = id,
            RefreshToken = refreshToken,
            UpdatedAt = DateTime.UtcNow
        };

        _databaseContext.Set<User>().Attach(user);
        _databaseContext.Entry(user).Property(x => x.RefreshToken).IsModified = true;
        _databaseContext.Entry(user).Property(x => x.UpdatedAt).IsModified = true;

        await _databaseContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<PaginatedList<User>> ListAllAsync(int page, int limit, string? name, CancellationToken cancellationToken)
    {
        var query = _databaseContext.Users
         .Where(x => string.IsNullOrWhiteSpace(name) || EF.Functions.Like(x.Name, name))
         .AsNoTracking();

        return await ListPaginatedAsync(query, page, limit, cancellationToken);
    }
}