using Ardalis.Result;
using JiuJitsuMaster.Core.Interfaces.Repositories;
using Microsoft.AspNetCore.Http;
using Octokit;

namespace JiuJitsuMaster.Infrastructure.Repositories;

public class GitHubStorageRepository : IGitHubStorageRepository
{
    private readonly GitHubClient _gitHubClient;

    public GitHubStorageRepository()
    {
        _gitHubClient = new GitHubClient(new ProductHeaderValue("JiuJitsuMaster"));
        var tokenAuth = new Credentials(Environment.GetEnvironmentVariable("GITHUB_TOKEN"));
        _gitHubClient.Credentials = tokenAuth;
    }

    public async Task<Result<string>> AddImageAsync(IFormFile imageFile, CancellationToken cancellationToken)
    {
        try
        {
            var repoOwner = "TarcioPassosFreitas";
            var repoName = "Images";
            var branchName = "main";
            var pathToImage = $"images/{Guid.NewGuid()}.jpg";

            byte[] imageBytes;
            using (var ms = new MemoryStream())
            {
                await imageFile.CopyToAsync(ms, cancellationToken);
                imageBytes = ms.ToArray();
            }

            var branch = await _gitHubClient.Repository.Branch.Get(repoOwner, repoName, branchName);
            var latestCommitSha = branch.Commit.Sha;

            var blob = new NewBlob
            {
                Encoding = EncodingType.Base64,
                Content = Convert.ToBase64String(imageBytes)
            };
            var blobRef = await _gitHubClient.Git.Blob.Create(repoOwner, repoName, blob);

            var tree = new NewTree { BaseTree = latestCommitSha };
            tree.Tree.Add(new NewTreeItem
            {
                Type = TreeType.Blob,
                Mode = Octokit.FileMode.File,
                Path = pathToImage,
                Sha = blobRef.Sha
            });
            var treeRef = await _gitHubClient.Git.Tree.Create(repoOwner, repoName, tree);

            var commit = new NewCommit("Add image", treeRef.Sha, latestCommitSha);
            var commitRef = await _gitHubClient.Git.Commit.Create(repoOwner, repoName, commit);

            await _gitHubClient.Git.Reference.Update(repoOwner, repoName, $"heads/{branchName}", new ReferenceUpdate(commitRef.Sha));

            var imageUrl = $"https://github.com/{repoOwner}/{repoName}/blob/{branchName}/{pathToImage}";

            return Result<string>.Success(imageUrl);
        }
        catch (Exception ex)
        {
            return Result<string>.Error(ex.Message);
        }
    }

    public async Task<Result> DeleteFileAsync(string fileName)
    {
        try
        {
            var repoOwner = "TarcioPassosFreitas";
            var repoName = "Images";
            var branchName = "main";

            var branch = await _gitHubClient.Repository.Branch.Get(repoOwner, repoName, branchName);
            var latestCommitSha = branch.Commit.Sha;

            var fileContents = await _gitHubClient.Repository.Content.GetAllContentsByRef(repoOwner, repoName, $"images/{fileName}", branchName);
            var fileSha = fileContents[0].Sha;

            var deleteFileRequest = new DeleteFileRequest($"Deleted file {fileName}", fileSha, latestCommitSha);

            await _gitHubClient.Repository.Content.DeleteFile(repoOwner, repoName, $"images/{fileName}", deleteFileRequest);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Error(ex.Message);
        }
    }


}