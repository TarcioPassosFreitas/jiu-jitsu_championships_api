using Ardalis.Result;
using AutoMapper;
using CommonUtility.Extensions;
using CommonUtility.Model;
using JiuJitsuMaster.Core.Aggregates;
using JiuJitsuMaster.Core.Common;
using JiuJitsuMaster.Core.Interfaces.Services;
using JiuJitsuMaster.UserAPI.Models.Atletes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace JiuJitsuMaster.UserAPI.Controllers;

[Route("[controller]")]
public class AthleteController : JiuJtsuControllerBase
{
    [AllowAnonymous]
    [HttpPost("Register/{championshipId:long}/Championship")]
    [ProducesResponseType(typeof(BaseModel<MessageModel>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(BaseModel<MessageModel>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseModel<MessageModel>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Register(
    [FromServices] IMapper mapper,
    [FromServices] IAthleteService userService,
    [FromRoute] long championshipId,
    [FromQuery] AthleteRequestModel request,
    [FromQuery] string captchaToken,
    CancellationToken cancellationToken)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToInvalidResult();
                Log.Warning("Estado do modelo inválido: {Errors}", errors);
                return BadRequest(BaseModel<MessageModel>.Failed(errors));
            }

            var athlete = mapper.Map<Athlete>(request);

            var response = await userService.RegisterUsersAsync(championshipId, athlete, captchaToken, cancellationToken);

            switch (response.Status)
            {
                case ResultStatus.Invalid:
                    Log.Warning("Dados inválidos para o cadastro: {ValidationErrors}", response.ValidationErrors);
                    return BadRequest(response.ValidationErrors);

                case ResultStatus.NotFound:
                    Log.Warning("Usuário não encontrado");
                    return NotFound(response.ValidationErrors);

                case ResultStatus.Unauthorized:
                    Log.Warning("Tentativa de convite não autorizada");
                    return Unauthorized();

                case ResultStatus.Error:
                    Log.Error("Erro interno durante o convite");
                    return InternalServerError();

                default:
                    Log.Information("Usuário registrado com sucesso");
                    return Created("User", BaseModel<MessageModel>.Success(new MessageModel("Usuário registrado com sucesso")));
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Uma exceção ocorreu durante a tentativa de Registro");
            return InternalServerError();
        }
    }
}