using Ardalis.Result;
using CommonUtility.Extensions;
using CommonUtility.Interfaces;
using JiuJitsuMaster.Core.Aggregates;
using JiuJitsuMaster.Core.Enums;
using JiuJitsuMaster.Core.ValueObjects;
using Microsoft.AspNetCore.Http;
using System.Drawing.Printing;
using System.Threading;

namespace JiuJitsuMaster.Core.Interfaces.Services;

public interface IChampionshipService : IBaseService<Championship>
{
    Task<Result<Championship>> AddAsync(Championship championship, IFormFile imageFile, 
        int? x, int? y, int? width, int? height, CancellationToken cancellationToken);

    Task<Result<PaginatedList<Championship>>> ListAllAsync(int page, int limit, string? title,
        string? city, string? state, ChampionshipStatus? status, CancellationToken cancellationToken);

    Task<Result<Championship>> UpdateAsync(Championship? championship, IFormFile? imageFile,
            int? x, int? y, int? width, int? height, CancellationToken cancellationToken);

    Task<Result<Championship>> ExcludeAsync(long id, CancellationToken cancellationToken);

    Task<Result<List<Championship>>> GetHighlightedAsync(CancellationToken cancellationToken);

    Task<Result> UpdateHighlightsAsync(List<long> highlights, CancellationToken cancellationToken);

    Task<Result<Championship>> ChangePhaseAsync(long id, ChampionshipPhase phase, CancellationToken cancellationToken);

    Task<Result> RecordFightResultsAsync(long championshipId, List<long> fightResultsIds, CancellationToken cancellationToken);

    Task<Result<FileData>> ExportResultsAsync(long championshipId, CancellationToken cancellationToken);

    Task<Result<List<Championship>>> GetNonHighlightedAsync(int page, int limit, string? state, string? city,
        ChampionshipType? type, string? title ,CancellationToken cancellationToken);

    Task<Result<List<Championship>>> GetFightKeysAsync(long championshipId, CancellationToken cancellationToken);
}