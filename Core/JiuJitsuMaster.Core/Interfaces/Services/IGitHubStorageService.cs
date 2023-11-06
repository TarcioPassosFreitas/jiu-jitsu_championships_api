using Ardalis.Result;
using Microsoft.AspNetCore.Http;

namespace JiuJitsuMaster.Core.Interfaces.Services;

public interface IGitHubStorageService
{
    Task<Result<string>> AddImageAsync(IFormFile imageFile, int? x, int? y, int? width, int? height, CancellationToken cancellationToken);
    Task<Result> RemoveImageAsync(string fileName);
}