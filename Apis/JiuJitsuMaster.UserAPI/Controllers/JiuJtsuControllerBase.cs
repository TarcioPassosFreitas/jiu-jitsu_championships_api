using Ardalis.Result;
using CommonUtility.Model;
using JiuJitsuMaster.UserAPI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using System.Security.Claims;

namespace JiuJitsuMaster.UserAPI.Controllers;

[ApiController]
[Produces(MediaTypeNames.Application.Json)]
[Consumes(MediaTypeNames.Application.Json)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public abstract class JiuJtsuControllerBase : ControllerBase
{
    protected ObjectResult InternalServerError()
    {
        var error = new ErrorModel
        {
            Code = "JJ-SRV5000",
            Description = "Internal Server Error",
            Message = "Erro"
        };

        var errors = new List<ErrorModel> { error };

        return StatusCode(StatusCodes.Status500InternalServerError, BaseModel<string>.Failed(errors));
    }

    protected NotFoundObjectResult EntityNotFound(List<ValidationError> validationErrors)
    {
        var errors = new List<ErrorModel>();

        foreach (var error in validationErrors)
            errors.Add(new ErrorModel
            {
                Code = error.Identifier,
                Description = error.ErrorCode,
                Message = error.ErrorMessage
            });

        return NotFound(BaseModel<string>.Failed(errors));
    }

    protected NotFoundObjectResult EntityNotFound(string message)
    {
        var error = new ErrorModel
        {
            Code = "JJ-CRD4040",
            Description = "Entidade não encontrada",
            Message = message
        };

        var errors = new List<ErrorModel> { error };

        return NotFound(BaseModel<string>.Failed(errors));
    }

    protected new UnauthorizedObjectResult Unauthorized()
    {
        var error = new ErrorModel
        {
            Code = "JJ-AUT4030",
            Description = "Solicitação não autorizada",
            Message = "Não autorizado"
        };

        var errors = new List<ErrorModel> { error };

        return Unauthorized(BaseModel<string>.Failed(errors));
    }

    protected ObjectResult Forbidden()
    {
        var error = new ErrorModel
        {
            Code = "JJ-AUT4030",
            Description = "Solicitação proibida",
            Message = "Proibido"
        };

        var errors = new List<ErrorModel> { error };

        return StatusCode(StatusCodes.Status403Forbidden, BaseModel<string>.Failed(errors));
    }

    protected new ObjectResult Conflict()
    {
        var error = new ErrorModel
        {
            Code = "JJ-CRD4090",
            Description = "Conflito de Duplicação de Entidade",
            Message = "Conflito"
        };

        var errors = new List<ErrorModel> { error };

        return StatusCode(StatusCodes.Status409Conflict, BaseModel<string>.Failed(errors));
    }

    protected ObjectResult BadRequest(List<ValidationError> validationErrors)
    {
        var errors = validationErrors.Select(error => new ErrorModel
        { Code = error.Identifier, Description = error.ErrorCode, Message = error.ErrorMessage }).ToList();

        return BadRequest(BaseModel<string>.Failed(errors));
    }

    protected ObjectResult BadRequest(string message)
    {
        var errors = new List<ErrorModel>
        {
            new()
            {
                Code = "JJ-SCH4000",
                Description = "Bad Request",
                Message = message
            }
        };

        return BadRequest(BaseModel<string>.Failed(errors));
    }

    protected Result<string> GetIdUser()
    {
        var user = User.Identity as ClaimsIdentity;

        var claims = user?.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);

        if (claims == null)
            return Result.Unauthorized();

        return Result.Success(claims.Value);
    }

    protected Result<string> GetRoleUser()
    {
        var user = User.Identity as ClaimsIdentity;

        var claims = user?.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role);

        if (claims == null)
            return Result.Unauthorized();

        return Result.Success(claims.Value);
    }
}