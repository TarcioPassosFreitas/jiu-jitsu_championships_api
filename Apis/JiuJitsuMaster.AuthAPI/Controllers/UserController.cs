using Ardalis.Result;
using CommonUtility.Extensions;
using CommonUtility.Model;
using JiuJitsuMaster.AuthAPI.Models;
using JiuJitsuMaster.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace JiuJitsuMaster.AuthAPI.Controllers;

[Route("[controller]")]
public class UserController : JiuJitsuControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromServices] ITokenService tokenService,
        [FromServices] IUserService userService,
        [FromBody] LoginRequestModel requestModel,
        CancellationToken cancellationToken)
    {
        Log.Information("Método de login chamado para o e-mail {Email}", requestModel.Email);

        if (!ModelState.IsValid)
        {
            var errors = ModelState.ToInvalidResult();
            Log.Warning("Estado do modelo inválido: {Errors}", errors);
            return BadRequest(BaseModel<LoginResponseModel>.Failed(errors));
        }

        try
        {
            var resultUser = await userService.LoginAsync(requestModel.Email, requestModel.Password, cancellationToken);

            if (resultUser.Status == ResultStatus.NotFound)
            {
                Log.Warning("Usuário não encontrado para o e-mail {Email}", requestModel.Email);
                return EntityNotFound(resultUser.ValidationErrors);
            }

            if (resultUser.Status == ResultStatus.Unauthorized)
            {
                Log.Warning("Tentativa de login não autorizada para o e-mail {Email}", requestModel.Email);
                return Unauthorized();
            }

            var user = resultUser.Value;
            var accessToken = tokenService.GenerateAccessToken(user, Environment.GetEnvironmentVariable("SECRET_ACCESS_TOKEN"));
            Log.Information("Usuário {Email} fez login com sucesso", requestModel.Email);

            return Ok(BaseModel<LoginResponseModel>.Success(new LoginResponseModel
            {
                AccessToken = accessToken,
                RefreshToken = user.RefreshToken!
            }));
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Uma exceção ocorreu durante a tentativa de login para o e-mail {Email}", requestModel.Email);
            return InternalServerError();
        }
    }

    [AllowAnonymous]
    [HttpPost("refreshToken")]
    public async Task<IActionResult> RefreshToken(
       [FromServices] IUserService userService,
       [FromServices] ITokenService tokenService,
       [FromBody] string refreshToken,
       CancellationToken cancellationToken)
    {
        Log.Information("Método de refreshToken chamado");

        if (!ModelState.IsValid)
        {
            var errors = ModelState.ToInvalidResult();
            Log.Warning("Estado do modelo inválido: {Errors}", errors);
            return BadRequest(BaseModel<LoginResponseModel>.Failed(errors));
        }

        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            ModelState.AddModelError("refreshToken", "RefreshToken é necessário");
            var errors = ModelState.ToInvalidResult();
            Log.Warning("RefreshToken não fornecido");
            return BadRequest(BaseModel<LoginResponseModel>.Failed(errors));
        }

        try
        {
            var resultUser = await userService.GetByRefreshTokenAsync(refreshToken, cancellationToken);

            if (resultUser.Status == ResultStatus.NotFound)
            {
                Log.Warning("Usuário não encontrado para o refreshToken fornecido");
                return EntityNotFound(resultUser.ValidationErrors);
            }

            if (resultUser.Status == ResultStatus.Unauthorized)
            {
                Log.Warning("Tentativa de atualização de token não autorizada");
                return Unauthorized();
            }

            var user = resultUser.Value;
            var accessToken = tokenService.GenerateAccessToken(user, Environment.GetEnvironmentVariable("SECRET_ACCESS_TOKEN"));
            Log.Information("Token atualizado com sucesso");

            return Ok(BaseModel<LoginResponseModel>.Success(new LoginResponseModel
            {
                AccessToken = accessToken,
                RefreshToken = user.RefreshToken!
            }));
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Uma exceção ocorreu durante a tentativa de atualização do token");
            return InternalServerError();
        }
    }

    [AllowAnonymous]
    [HttpPost("sendRecoveryPasswordEmail")]
    public async Task<IActionResult> ForgotPassword(
        [FromServices] IUserRecoverPasswordService userRecoverPasswordService,
        [FromBody] ForgotPasswordRequestModel requestModel,
        CancellationToken cancellationToken)
    {
        Log.Information("Método de recuperação de senha chamado para o e-mail {Email}", requestModel.Email);

        if (!ModelState.IsValid)
        {
            var errors = ModelState.ToInvalidResult();
            Log.Warning("Estado do modelo inválido: {Errors}", errors);
            return BadRequest(BaseModel<MessageModel>.Failed(errors));
        }

        try
        {
            var result = await userRecoverPasswordService.AddAsync(requestModel.Email,
                new HostString(Environment.GetEnvironmentVariable("PLATAFORMA_URL")), cancellationToken);

            if (result.Status == ResultStatus.NotFound || result.Value == null)
            {
                Log.Warning("Usuário não encontrado para o e-mail {Email}", requestModel.Email);
                return EntityNotFound(result.ValidationErrors);
            }

            if (result.Status == ResultStatus.Error)
            {
                Log.Error("Erro no envio do e-mail de recuperação de senha");
                return EntityNotFound("Erro no envio do e-mail");
            }

            Log.Information("E-mail de recuperação de senha enviado com sucesso para {Email}", requestModel.Email);
            return Ok(BaseModel<MessageModel>.Success(new MessageModel("Enviado")));
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Uma exceção ocorreu durante a tentativa de recuperação de senha para o e-mail {Email}", requestModel.Email);
            return InternalServerError();
        }
    }
}