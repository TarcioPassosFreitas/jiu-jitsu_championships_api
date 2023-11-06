using Ardalis.Result;
using CommonUtility.Extensions;
using CommonUtility.Interfaces;
using JiuJitsuMaster.Core.Aggregates;
using JiuJitsuMaster.Core.Enums;

namespace JiuJitsuMaster.Core.Interfaces.Repositories;

public interface IChampionshipRepository : IBaseRepository<Championship>
{
    Task<List<string>> GetByDetailsAsync(
    string? title, string? state, string? city, string? gym, DateTimeOffset? eventDate,
    CancellationToken cancellationToken);

    Task<PaginatedList<Championship>> ListAllAsync(int page, int limit, string? title,
        string? city, string? state, ChampionshipStatus? status, CancellationToken cancellationToken);

    Task<List<Championship>> GetHighlightedAsync(CancellationToken cancellationToken);

    Task<bool> GetByIdsAsync(List<long> ids, CancellationToken cancellationToken);

    Task UpdatePreviouslyHighlightedAsync(long id, bool highlight, int? highlightOrder, CancellationToken cancellationToken);

    Task ChangePhaseAsync(long id, ChampionshipPhase phase, CancellationToken cancellationToken);

    Task<List<Championship>> GetNonHighlightedAsync(int page, int limit, string? state, string? city,
        ChampionshipType? type, string? title, CancellationToken cancellationToken);

    Task<List<Championship>> GetFightKeysAsync(long championshipId, CancellationToken cancellationToken);
}