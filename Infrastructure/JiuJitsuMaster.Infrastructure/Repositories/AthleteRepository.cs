using CommonUtility.Extensions;
using CommonUtility.Repository;
using JiuJitsuMaster.Core.Aggregates;
using JiuJitsuMaster.Core.Enums;
using JiuJitsuMaster.Core.Interfaces.Repositories;
using JiuJitsuMaster.Infrastructure.Database.Contexts;
using Microsoft.EntityFrameworkCore;

namespace JiuJitsuMaster.Infrastructure.Repositories;

public class AthleteRepository : BaseRepository<Athlete>, IAthleteRepository
{
    private readonly DatabaseContext _databaseContext;

    public AthleteRepository(DatabaseContext dbContext)
        : base(dbContext)
    {
        _databaseContext = dbContext;
    }

    public async Task ChangePasswordAsync(long id, string newPassword, CancellationToken cancellationToken)
    {
        var athlete = new Athlete
        {
            Id = id,
            Password = newPassword,
            UpdatedAt = DateTime.UtcNow
        };

        _databaseContext.Set<Athlete>().Attach(athlete);
        _databaseContext.Entry(athlete).Property(x => x.Password).IsModified = true;
        _databaseContext.Entry(athlete).Property(x => x.UpdatedAt).IsModified = true;

        await _databaseContext.SaveChangesAsync(cancellationToken);
    }


    public async Task<Athlete?> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return await _databaseContext.Athletes
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Email.Equals(email), cancellationToken);
    }

    public async Task<Athlete?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
    {
        return await _databaseContext.Athletes
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.RefreshToken == refreshToken, cancellationToken);
    }

    public async Task UpdateRefreshTokenAsync(long id, string refreshToken, CancellationToken cancellationToken)
    {
        var athlete = new Athlete
        {
            Id = id,
            RefreshToken = refreshToken,
            UpdatedAt = DateTime.UtcNow
        };

        _databaseContext.Set<Athlete>().Attach(athlete);
        _databaseContext.Entry(athlete).Property(x => x.RefreshToken).IsModified = true;
        _databaseContext.Entry(athlete).Property(x => x.UpdatedAt).IsModified = true;

        await _databaseContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<PaginatedList<Athlete>> GetAthleteRegistrationsAsync(int page, int pageSize, string? athleteName, Gender? gender, string? championshipTitle, string? city, string? state, CancellationToken cancellationToken)
    {
        var baseQuery = _databaseContext.AthleteChampionships
            .Include(x => x.Championship)
            .Include(x => x.Athlete)
            .AsNoTracking()
            .Where(c => string.IsNullOrEmpty(championshipTitle) || c.Championship.Title.Contains(championshipTitle))
            .Where(c => string.IsNullOrEmpty(city) || c.Championship.City.Contains(city))
            .Where(c => string.IsNullOrEmpty(state) || c.Championship.State.Contains(state))
            .Where(x => string.IsNullOrEmpty(athleteName) || x.Athlete.Name.Contains(athleteName))
            .Where(x => !gender.HasValue || x.Athlete.Gender == gender)
            .Select(ac => ac.Athlete);

        return await ListPaginatedAsync(baseQuery, page, pageSize, cancellationToken);
    }

    public async Task<List<Athlete>> GetAthleteRegistrationsAsync(string? athleteName, Gender? gender, string? championshipTitle, string? city, string? state, CancellationToken cancellationToken)
    {
        return await _databaseContext.AthleteChampionships
            .Include(x => x.Championship)
            .Include(x => x.Athlete)
            .AsNoTracking()
            .Where(c => string.IsNullOrEmpty(championshipTitle) || c.Championship.Title.Contains(championshipTitle))
            .Where(c => string.IsNullOrEmpty(city) || c.Championship.City.Contains(city))
            .Where(c => string.IsNullOrEmpty(state) || c.Championship.State.Contains(state))
            .Where(x => string.IsNullOrEmpty(athleteName) || x.Athlete.Name.Contains(athleteName))
            .Where(x => !gender.HasValue || x.Athlete.Gender == gender)
            .Select(ac => ac.Athlete)
            .ToListAsync(cancellationToken);
    }
}