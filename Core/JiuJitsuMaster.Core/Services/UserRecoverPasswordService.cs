using Ardalis.Result;
using CommonUtility.Service;
using JiuJitsuMaster.Core.Aggregates;
using JiuJitsuMaster.Core.Interfaces.Repositories;
using JiuJitsuMaster.Core.Interfaces.Services;
using Microsoft.AspNetCore.Http;

namespace JiuJitsuMaster.Core.Services;

public class UserRecoverPasswordService : BaseService<UserRecoverPassword>, IUserRecoverPasswordService
{
    private readonly IMailKitService _mailKitService;
    private readonly ITokenService _tokenService;
    private readonly IUserRecoverPasswordRepository _userRecoverPasswordRepository;
    private readonly IUserService _userService;
    private readonly IAthleteService _athleteService;

    public UserRecoverPasswordService(
        IUserRecoverPasswordRepository userRecoverPasswordRepository,
        IMailKitService mailKitService,
        IUserService userService,
        IAthleteService athleteService,
        ITokenService tokenService)
    {
        _userRecoverPasswordRepository = userRecoverPasswordRepository;
        _mailKitService = mailKitService;
        _userService = userService;
        _athleteService = athleteService;
        _tokenService = tokenService;
    }

    public async Task<Result<UserRecoverPassword>> AddAsync(string email, HostString host,
        CancellationToken cancellationToken)
    {
        var resultUser = await _userService.GetByEmailAsync(email, cancellationToken);

        var resultAthlete = await _athleteService.GetByEmailAsync(email, cancellationToken);

        if (resultUser.IsSuccess)
        {
            var user = resultUser.Value;

            if (user == null || user.Deleted) return NotFound("Usuário não encontrado");

            var userRecoverPassword = new UserRecoverPassword
            {
                CreatedAt = DateTime.Now,
                UserId = user.Id,
                Expire = DateTime.UtcNow.AddHours(2),
                Token = _tokenService.GenerateUUIDToken(),
                UpdatedAt = DateTime.Now
            };

            userRecoverPassword = await _userRecoverPasswordRepository.AddAsync(userRecoverPassword, cancellationToken);

            var url = $"https://{host}/account/changeRecoveryPassword?token={userRecoverPassword.Token}";

            var emailBody = $"Olá {user.Name}, clique no link para redefinir sua senha: {url}";

            var mailSent = await _mailKitService.SendMessageAsync(user.Email, $"{user.Name}", "Recuperação de Senha", emailBody);

            if (mailSent)
                return Result<UserRecoverPassword>.Success(userRecoverPassword);
            return Result<UserRecoverPassword>.Error();
        }
        else if (resultAthlete.IsSuccess)
        {
            var athlete = resultAthlete.Value;

            if (athlete == null || athlete.Deleted) return NotFound("Usuário não encontrado");

            var userRecoverPassword = new UserRecoverPassword
            {
                CreatedAt = DateTime.Now,
                AtleteId = athlete.Id,
                Expire = DateTime.UtcNow.AddHours(2),
                Token = _tokenService.GenerateUUIDToken(),
                UpdatedAt = DateTime.Now
            };

            userRecoverPassword = await _userRecoverPasswordRepository.AddAsync(userRecoverPassword, cancellationToken);

            var url = $"https://{host}/account/changeRecoveryPassword?token={userRecoverPassword.Token}";

            var emailBody = $"Olá {athlete.Name}, clique no link para redefinir sua senha: {url}";

            var mailSent = await _mailKitService.SendMessageAsync(athlete.Email, $"{athlete.Name}", "Recuperação de Senha", emailBody);

            if (mailSent)
                return Result<UserRecoverPassword>.Success(userRecoverPassword);
            return Result<UserRecoverPassword>.Error();
        }

        return NotFound("Usuário ou atleta não encontrado");
    }

    public async Task<Result<UserRecoverPassword>> ResetPasswordAsync(string token, string newPassword,
        string confirmPassword, CancellationToken cancellationToken)
    {
        var userRecoverPasswordResult = await _userRecoverPasswordRepository.GetByTokenAsync(token, cancellationToken);

        if (userRecoverPasswordResult == null || userRecoverPasswordResult.Deleted)
            return NotFound("O usuário não tem uma solicitação de alteração de senha");

        if (userRecoverPasswordResult.Expire < DateTime.UtcNow) return TokenExpired("Token expirado");

        if (userRecoverPasswordResult.UserId.HasValue)
        {
            var resultUser = await _userService.ResetPasswordAsync(userRecoverPasswordResult.UserId.Value, newPassword,
            confirmPassword, cancellationToken);

            if (resultUser.IsSuccess)
            {
                await _userRecoverPasswordRepository.ExpireToken(userRecoverPasswordResult.Id, DateTime.UtcNow,
                    cancellationToken);

                return Result.Success();
            }

            return Result.Invalid(resultUser.ValidationErrors);
        }

        var result = await _userService.ResetAthletePasswordAsync(userRecoverPasswordResult.AtleteId.Value, newPassword,
            confirmPassword, cancellationToken);

        if (result.IsSuccess)
        {
            await _userRecoverPasswordRepository.ExpireToken(userRecoverPasswordResult.Id, DateTime.UtcNow,
                cancellationToken);

            return Result.Success();
        }

        return Result.Invalid(result.ValidationErrors);
    }
}