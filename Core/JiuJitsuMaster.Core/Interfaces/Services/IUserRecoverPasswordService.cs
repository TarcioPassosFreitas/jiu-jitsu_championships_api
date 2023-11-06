using Ardalis.Result;
using CommonUtility.Interfaces;
using JiuJitsuMaster.Core.Aggregates;
using Microsoft.AspNetCore.Http;

namespace JiuJitsuMaster.Core.Interfaces.Services;

public interface IUserRecoverPasswordService : IBaseService<UserRecoverPassword>
{
    Task<Result<UserRecoverPassword>> AddAsync(string email, HostString host, CancellationToken cancellationToken);

    Task<Result<UserRecoverPassword>> ResetPasswordAsync(string token, string newPassword, string confirmPassword,
        CancellationToken cancellationToken);
}