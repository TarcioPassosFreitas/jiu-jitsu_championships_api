using Ardalis.Result;
using AutoMapper;
using CommonUtility.Extensions;
using CommonUtility.Model;
using JiuJitsuMaster.AdminApi.Controllers;
using JiuJitsuMaster.AdminAPI.Models.UserModel;
using JiuJitsuMaster.Core.Aggregates;
using JiuJitsuMaster.Core.Common;
using JiuJitsuMaster.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace JiuJitsuMaster.AdminAPI.Controllers;

[Route("[controller]")]
public class UserController : JiuJtsuControllerBase
{
    [Authorize(Roles = RoleNames.Administrator)]
    [HttpPost("Register")]
    [ProducesResponseType(typeof(BaseModel<MessageModel>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(BaseModel<MessageModel>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseModel<MessageModel>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Register(
    [FromServices] IMapper mapper,
    [FromServices] IUserService userService,
    [FromBody] RegisterRequestModel request,
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

            var user = mapper.Map<User>(request);

            var adminUserId = long.Parse(GetIdUser());

            var response = await userService.RegisterUsersAsync(user, cancellationToken);

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

    [Authorize(Roles = RoleNames.Administrator)]
    [HttpPut("{id:long}")]
    [ProducesResponseType(typeof(BaseModel<UserResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseModel<UserResponseModel>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseModel<UserResponseModel>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseModel<UserResponseModel>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> EditUser(
        [FromServices] IMapper mapper,
        [FromServices] IUserService userService,
        [FromRoute] long id,
        [FromBody] UserUpdateRequestModel userUpdateRequestModel,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.ToInvalidResult();
            return BadRequest(BaseModel<UserResponseModel>.Failed(errors));
        }

        var user = mapper.Map<User>(userUpdateRequestModel);
        user.Id = id;

        try
        {
            var userUpdate = await userService.UpdateUserAsync(user, cancellationToken);

            if (userUpdate.Status == ResultStatus.NotFound) return EntityNotFound(userUpdate.ValidationErrors);

            if (userUpdate.Status == ResultStatus.Unauthorized) return Unauthorized();

            if (userUpdate.Status == ResultStatus.Error) return InternalServerError();

            if (userUpdate.Status == ResultStatus.Invalid) return BadRequest(userUpdate.ValidationErrors);

            var response = mapper.Map<UserResponseModel>(userUpdate.Value);

            return Ok(BaseModel<UserResponseModel>.Success(response));
        }
        catch (Exception)
        {
            return InternalServerError();
        }
    }

    [Authorize(Roles = RoleNames.Administrator)]
    [HttpDelete("{id:long}")]
    [ProducesResponseType(typeof(BaseModel<MessageModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseModel<MessageModel>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseModel<MessageModel>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExcludeUser(
        [FromServices] IUserService userService,
        [FromRoute] long id,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = long.Parse(GetIdUser());

            var result = await userService.ExcludeAsync(id, cancellationToken);

            if (result.Status == ResultStatus.NotFound || result.Status == ResultStatus.Error) return EntityNotFound(result.ValidationErrors);

            return Ok(BaseModel<MessageModel>.Success(new("Usuário excluído com sucesso")));
        }
        catch (Exception)
        {
            return InternalServerError();
        }
    }

    [Authorize(Roles = RoleNames.Administrator)]
    [HttpGet("")]
    [ProducesResponseType(typeof(BaseModel<ListBaseModel<List<UserResponseModel>>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseModel<ListBaseModel<List<UserResponseModel>>>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ListPageAsync(
        [FromServices] IUserService userService,
        [FromServices] IMapper mapper,
        [FromQuery] int page,
        [FromQuery] int limit,
        [FromQuery] string? name,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.ToInvalidResult();
            return BadRequest(BaseModel<UserResponseModel>.Failed(errors));
        }

        try
        {
            if (page <= 0 || limit <= 0)
                return BadRequest("Página ou limite invalido");

            var paginatedPage = await userService.ListAllAsync(page, limit, name, cancellationToken);

            var data = paginatedPage.Value;

            if (paginatedPage.Status == ResultStatus.Invalid) return BadRequest(paginatedPage.ValidationErrors);

            var content = new List<UserResponseModel>();

            foreach (var user in data.ToList())
                content.Add(mapper.Map<UserResponseModel>(user));

            var response = new ListBaseModel<List<UserResponseModel>>(data.TotalCount, content, page, limit);

            return Ok(BaseModel<ListBaseModel<List<UserResponseModel>>>.Success(response));
        }
        catch (Exception)
        {
            return InternalServerError();
        }
    }
}