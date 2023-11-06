using CommonUtility.Extensions;
using CommonUtility.Repository;
using iTextSharp.text;
using JiuJitsuMaster.Core.Aggregates;
using JiuJitsuMaster.Core.Enums;
using JiuJitsuMaster.Core.Interfaces.Repositories;
using JiuJitsuMaster.Core.ValueObjects;
using JiuJitsuMaster.Infrastructure.Database.Contexts;
using JiuJitsuMaster.Infrastructure.Database.Migrations;
using Microsoft.EntityFrameworkCore;

namespace JiuJitsuMaster.Infrastructure.Repositories;

public class ChampionshipRepository : BaseRepository<Championship>, IChampionshipRepository
{
    private readonly DatabaseContext _databaseContext;

    public ChampionshipRepository(DatabaseContext dbContext)
        : base(dbContext)
    {
        _databaseContext = dbContext;
    }

    public async Task ChangePhaseAsync(long id, ChampionshipPhase phase, CancellationToken cancellationToken)
    {
        var championship = new Championship
        {
            Id = id,
            Phase = phase,
            UpdatedAt = DateTime.UtcNow
        };

        _databaseContext.Set<Championship>().Attach(championship);
        _databaseContext.Entry(championship).Property(x => x.Phase).IsModified = true;
        _databaseContext.Entry(championship).Property(x => x.UpdatedAt).IsModified = true;

        await _databaseContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<PaginatedList<Championship>> GetAthleteRegistrationsAsync(int page, int pageSize, string? athleteName, Gender? gender, string? championshipTitle, string? city, string? state, CancellationToken cancellationToken)
    {
        var baseQuery = _databaseContext.Championships.AsNoTracking();

        if (!string.IsNullOrEmpty(championshipTitle))
        {
            baseQuery = baseQuery.Where(c => c.Title.Contains(championshipTitle));
        }

        if (!string.IsNullOrEmpty(city))
        {
            baseQuery = baseQuery.Where(c => c.City.Contains(city));
        }

        if (!string.IsNullOrEmpty(state))
        {
            baseQuery = baseQuery.Where(c => c.State.Contains(state));
        }

        var athleteQuery = baseQuery
            .Include(c => c.AthleteChampionships)
            .ThenInclude(x => x.Athlete)
            .Where(x => !string.IsNullOrEmpty(athleteName) ? x.AthleteChampionships.Any(d => d.Athlete.Name.Contains(athleteName)) : true)
            .Where(x => gender.HasValue ? x.AthleteChampionships.Any(d => d.Athlete.Gender == gender) : true);
           

        return await ListPaginatedAsync(athleteQuery, page, pageSize, cancellationToken);
    }

    public async Task<List<string>> GetByDetailsAsync(
    string? title, string? state, string? city, string? gym, DateTimeOffset? eventDate,
    CancellationToken cancellationToken)
    {
        List<string> conflicts = new List<string>();

        var query = _databaseContext.Championships
                    .AsNoTracking()
                    .AsQueryable();

        if (title != null) query = query.Where(c => c.Title == title);
        if (state != null) query = query.Where(c => c.State == state);
        if (city != null) query = query.Where(c => c.City == city);
        if (gym != null) query = query.Where(c => c.Gym == gym);
        if (eventDate != null) query = query.Where(c => c.EventDate == eventDate);

        var result = await query.Select(c => new { c.Title, c.State, c.City, c.Gym, c.EventDate })
                                .ToListAsync(cancellationToken);

        if (result.Any(r => r.Title == title)) conflicts.Add("Title já existe");
        if (result.Any(r => r.State == state)) conflicts.Add("State já existe");
        if (result.Any(r => r.City == city)) conflicts.Add("City já existe");
        if (result.Any(r => r.Gym == gym)) conflicts.Add("Gym já existe");
        if (result.Any(r => r.EventDate == eventDate)) conflicts.Add("EventDate já existe");

        return conflicts;
    }

    public async Task<bool> GetByIdsAsync(List<long> ids, CancellationToken cancellationToken)
    {
        var count = await _databaseContext.Championships
            .AsNoTracking()
            .CountAsync(c => ids.Contains(c.Id) && c.Status == ChampionshipStatus.Active, cancellationToken);

        return count == ids.Count;
    }

    public async Task<List<Championship>> GetHighlightedAsync(CancellationToken cancellationToken)
    {
        return await _databaseContext.Championships
                             .AsNoTracking()
                             .Where(c => c.Highlights == true && c.Status == ChampionshipStatus.Active)
                             .OrderBy(c => c.HighlightOrder)
                             .Take(8)
                             .ToListAsync(cancellationToken);
    }

    public async Task<List<Championship>> GetNonHighlightedAsync(int page, int limit, string? state, string? city,
        ChampionshipType? type, string? title, CancellationToken cancellationToken)
    {
        var baseQuery = _databaseContext.Championships
            .Where(x => ((x.Highlights == false) && x.Status == ChampionshipStatus.Active))
            .Where(x => string.IsNullOrWhiteSpace(title) || EF.Functions.Like(x.Title, $"%{title}%"))
            .Where(x => string.IsNullOrWhiteSpace(city) || EF.Functions.Like(x.City, $"%{city}%"))
            .Where(x => string.IsNullOrWhiteSpace(state) || EF.Functions.Like(x.State, $"%{state}%"))
            .AsNoTracking();

        if (type.HasValue)
            baseQuery = baseQuery.Where(x => x.Type == type.Value);

        var orderedQuery = baseQuery.OrderByDescending(x => x.EventDate);

        return await ListPaginatedAsync(orderedQuery, page, limit, cancellationToken);
    }


    public async Task<PaginatedList<Championship>> ListAllAsync(int page, int limit, string? title,
        string? city, string? state, ChampionshipStatus? status, CancellationToken cancellationToken)
    {
        var baseQuery = _databaseContext.Championships
            .Where(x => string.IsNullOrWhiteSpace(title) || EF.Functions.Like(x.Title, $"%{title}%"))
            .Where(x => string.IsNullOrWhiteSpace(city) || EF.Functions.Like(x.City, $"%{city}%"))
            .Where(x => string.IsNullOrWhiteSpace(state) || EF.Functions.Like(x.State, $"%{state}%"))
            .Where(x => !status.HasValue || x.Status == status.Value)
            .AsNoTracking();

        return await ListPaginatedAsync(baseQuery, page, limit, cancellationToken);
    }

    public async Task UpdatePreviouslyHighlightedAsync(long id, bool highlight, int? highlightOrder, CancellationToken cancellationToken)
    {
        var championship = await _databaseContext.Championships
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (championship != null)
        {
            championship.Highlights = highlight;
            championship.HighlightOrder = highlightOrder;
            championship.UpdatedAt = DateTime.UtcNow;

            await _databaseContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<List<Championship>> GetFightKeysAsync(long championshipId, CancellationToken cancellationToken)
    {
        return await _databaseContext.Championships
            .AsNoTracking()
            .Include(x => x.Fights)
            .ThenInclude(x => x.AthleteFights)
            .Where(x => x.Id == championshipId && x.Highlights == true)
            .ToListAsync(cancellationToken);
    }
}