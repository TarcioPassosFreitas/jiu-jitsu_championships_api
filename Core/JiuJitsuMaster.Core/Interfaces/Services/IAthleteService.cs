using Ardalis.Result;
using CommonUtility.Extensions;
using CommonUtility.Interfaces;
using JiuJitsuMaster.Core.Aggregates;
using JiuJitsuMaster.Core.Enums;
using JiuJitsuMaster.Core.ValueObjects;

namespace JiuJitsuMaster.Core.Interfaces.Services;

public interface IAthleteService : IBaseService<Athlete>
{
    Task<Result<Athlete>> LoginAsync(string email, string password, CancellationToken cancellationToken);

    Task<Result<Athlete>> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken);

    Task<Result<Athlete>> ResetPasswordAsync(long id, string newPassword, string confirmPassword, CancellationToken cancellationToken);

    Task<Result<Athlete>> RegisterUsersAsync(long championshipId, Athlete athlete, string captchaToken, CancellationToken cancellationToken);

    Task<Result<PaginatedList<Athlete>>> GetAthleteRegistrationsAsync(
        int page, int pageSize, string? athleteName, Gender? gender, string? championshipTitle,
        string? city, string? state, CancellationToken cancellationToken);

    Task<Result<FileData>> ExportAthleteRegistrationsToCsvAsync(
        string? athleteName,
        Gender? gender,
        string? championshipTitle,
        string? city,
        string? state,
        CancellationToken cancellationToken);

    Task<Result<FileData>> ExportAthleteRegistrationsToPdfAsync(
        string? athleteName,
        Gender? gender,
        string? championshipTitle,
        string? city,
        string? state,
        CancellationToken cancellationToken);

    Task<Result<Athlete>> GetByEmailAsync(string email, CancellationToken cancellationToken);
}