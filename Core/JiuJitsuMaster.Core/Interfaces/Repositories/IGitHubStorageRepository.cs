using Ardalis.Result;
using Microsoft.AspNetCore.Http;

namespace JiuJitsuMaster.Core.Interfaces.Repositories;

public interface IGitHubStorageRepository
{
    Task<Result<string>> AddImageAsync(IFormFile imageFile, CancellationToken cancellationToken);
    Task<Result> DeleteFileAsync(string fileName);
}