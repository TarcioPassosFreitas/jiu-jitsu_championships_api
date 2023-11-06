using Ardalis.Result;
using AutoMapper;
using CommonUtility.Extensions;
using CommonUtility.Model;
using JiuJitsuMaster.AdminApi.Controllers;
using JiuJitsuMaster.AdminAPI.Models.ChampionshipModel;
using JiuJitsuMaster.Core.Aggregates;
using JiuJitsuMaster.Core.Common;
using JiuJitsuMaster.Core.Enums;
using JiuJitsuMaster.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JiuJitsuMaster.AdminAPI.Controllers;

[Route("[controller]")]
public class ChampionshipController : JiuJtsuControllerBase
{
    [Authorize(Roles = RoleNames.Administrator + "," + RoleNames.User)]
    [HttpPost("RegisterChampionship")]
    [ProducesResponseType(typeof(BaseModel<MessageModel>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(BaseModel<MessageModel>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseModel<MessageModel>), StatusCodes.Status404NotFound)]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> RegisterChampionship(
    [FromServices] IMapper mapper,
    [FromServices] IChampionshipService championshipService,
    [FromForm] ChampionshipRequestModel request,
    CancellationToken cancellationToken)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToInvalidResult();
                return BadRequest(BaseModel<MessageModel>.Failed(errors));
            }

            var championship = mapper.Map<Championship>(request);

            var response = await championshipService.AddAsync(championship, request.ImageFiles,
                request.X, request.Y, request.Width, request.Height, cancellationToken);

            switch (response.Status)
            {
                case ResultStatus.Invalid:
                    return BadRequest(response.ValidationErrors);
                case ResultStatus.NotFound:
                    return NotFound(response.ValidationErrors);
                case ResultStatus.Error:
                    return StatusCode(StatusCodes.Status500InternalServerError);
                default:
                    return Created("Championship", BaseModel<MessageModel>.Success(new MessageModel("Campeonato registrado com sucesso")));
            }
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [Authorize(Roles = RoleNames.Administrator + "," + RoleNames.User)]
    [HttpGet("")]
    [ProducesResponseType(typeof(BaseModel<ListBaseModel<List<ChampionshipResponseModel>>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseModel<ListBaseModel<List<ChampionshipResponseModel>>>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ListPageAsync(
        [FromServices] IChampionshipService championshipService,
        [FromServices] IMapper mapper,
        [FromQuery] int page,
        [FromQuery] int limit,
        [FromQuery] string? title,
        [FromQuery] string? city,
        [FromQuery] string? state,
        [FromQuery] ChampionshipStatus? status,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.ToInvalidResult();
            return BadRequest(BaseModel<ChampionshipResponseModel>.Failed(errors));
        }

        try
        {
            if (page <= 0 || limit <= 0)
                return BadRequest("Página ou limite invalido");

            var paginatedPage = await championshipService.ListAllAsync(page, limit, title, city, state, status, cancellationToken);

            var data = paginatedPage.Value;

            if (paginatedPage.Status == ResultStatus.Invalid) return BadRequest(paginatedPage.ValidationErrors);

            var content = new List<ChampionshipResponseModel>();

            foreach (var user in data.ToList())
                content.Add(mapper.Map<ChampionshipResponseModel>(user));

            var response = new ListBaseModel<List<ChampionshipResponseModel>>(data.TotalCount, content, page, limit);

            return Ok(BaseModel<ListBaseModel<List<ChampionshipResponseModel>>>.Success(response));
        }
        catch (Exception)
        {
            return InternalServerError();
        }
    }

    [Authorize(Roles = RoleNames.Administrator)]
    [HttpPut("{id:long}")]
    [ProducesResponseType(typeof(BaseModel<ChampionshipResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseModel<ChampionshipResponseModel>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseModel<ChampionshipResponseModel>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseModel<ChampionshipResponseModel>), StatusCodes.Status401Unauthorized)]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> EditChampionship(
       [FromServices] IMapper mapper,
       [FromServices] IChampionshipService championshipService,
       [FromRoute] long id,
       [FromForm] ChampionshipUpdateRequestModel request,
       CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.ToInvalidResult();
            return BadRequest(BaseModel<ChampionshipResponseModel>.Failed(errors));
        }

        if (request.GetType().GetProperties().All(p => p.GetValue(request) == null))
        {
            var newError = new ErrorModel
            {
                Code = "JJ-SCH4000",
                Description = "Nenhum campo para atualizar foi enviado.",
                Message = "Nenhum campo para atualizar foi enviado."
            };

            var errors = ModelState.ToInvalidResult().Concat(new[] { newError });

            return BadRequest(BaseModel<ChampionshipResponseModel>.Failed(errors));
        }

        var championship = mapper.Map<Championship>(request);
        championship.Id = id;

        try
        {
            var userUpdate = await championshipService.UpdateAsync(championship, request.ImageFiles,
                request.X, request.Y, request.Width, request.Height, cancellationToken);

            if (userUpdate.Status == ResultStatus.NotFound) return EntityNotFound(userUpdate.ValidationErrors);

            if (userUpdate.Status == ResultStatus.Unauthorized) return Unauthorized();

            if (userUpdate.Status == ResultStatus.Error) return InternalServerError();

            if (userUpdate.Status == ResultStatus.Invalid) return BadRequest(userUpdate.ValidationErrors);

            var response = mapper.Map<ChampionshipResponseModel>(userUpdate.Value);

            return Ok(BaseModel<ChampionshipResponseModel>.Success(response));
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
    public async Task<IActionResult> Exclude(
        [FromServices] IChampionshipService championshipService,
        [FromRoute] long id,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await championshipService.ExcludeAsync(id, cancellationToken);

            if (result.Status == ResultStatus.NotFound || result.Status == ResultStatus.Error) return EntityNotFound(result.ValidationErrors);

            return Ok(BaseModel<MessageModel>.Success(new("Campeonato excluído com sucesso")));
        }
        catch (Exception)
        {
            return InternalServerError();
        }
    }

    [Authorize(Roles = RoleNames.Administrator + "," + RoleNames.User)]
    [HttpGet("GetHighligh")]
    [ProducesResponseType(typeof(BaseModel<ListBaseModel<List<ChampionshipResponseModel>>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseModel<ListBaseModel<List<ChampionshipResponseModel>>>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetHighligh(
       [FromServices] IMapper mapper,
       [FromServices] IChampionshipService championshipService,
       CancellationToken cancellationToken)
    {
        try
        {
            var result = await championshipService.GetHighlightedAsync(cancellationToken);

            if (result.Status == ResultStatus.NotFound)
                return EntityNotFound(result.ValidationErrors);

            var response = mapper.Map<List<ChampionshipResponseModel>>(result.Value);

            return Ok(BaseModel<List<ChampionshipResponseModel>>.Success(response));
        }
        catch (Exception)
        {
            return InternalServerError();
        }
    }

    [Authorize(Roles = RoleNames.Administrator)]
    [HttpPut("updateHighlights")]
    [ProducesResponseType(typeof(BaseModel<MessageModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseModel<MessageModel>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseModel<MessageModel>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateHighlights(
    [FromServices] IChampionshipService championshipService,
    [FromQuery] List<long> highlights,
    CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.ToInvalidResult();
            return BadRequest(BaseModel<ChampionshipResponseModel>.Failed(errors));
        }

        try
        {
            var result = await championshipService.UpdateHighlightsAsync(highlights, cancellationToken);

            if (result.Status == ResultStatus.NotFound) return EntityNotFound(result.ValidationErrors);

            if (result.Status == ResultStatus.Unauthorized) return Unauthorized();

            if (result.Status == ResultStatus.Error) return InternalServerError();

            if (result.Status == ResultStatus.Invalid) return BadRequest(result.ValidationErrors);

            return Ok(BaseModel<MessageModel>.Success(new MessageModel("Campeonatos atualizados com sucesso")));
        }
        catch (Exception)
        {
            return InternalServerError();
        }
    }

    [Authorize(Roles = RoleNames.Administrator)]
    [HttpPatch("Championship/{id:long}/changePhase")]
    [ProducesResponseType(typeof(BaseModel<MessageModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseModel<MessageModel>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangeChampionshipPhase(
    [FromServices] IChampionshipService championshipService,
    [FromRoute] long id,
    [FromQuery] ChampionshipPhaseRequestModel championshipPhaseModel,
    CancellationToken cancellationToken)
    {
        try
        {
            var result = await championshipService.ChangePhaseAsync(id, championshipPhaseModel.Phase, cancellationToken);

            switch (result.Status)
            {
                case ResultStatus.NotFound:
                    return EntityNotFound(result.ValidationErrors);

                case ResultStatus.Error:
                    return Conflict(result.ValidationErrors);
            }

            return Ok(BaseModel<MessageModel>.Success(new MessageModel("Fase do campeonato alterada com sucesso")));
        }
        catch (Exception)
        {
            return InternalServerError();
        }
    }

    [Authorize(Roles = RoleNames.Administrator)]
    [HttpPost("{championshipId:long}/Championship/fights/results")]
    [ProducesResponseType(typeof(BaseModel<MessageModel>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(BaseModel<MessageModel>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseModel<MessageModel>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RecordFightResults(
    [FromServices] IChampionshipService championshipService,
    [FromRoute] long championshipId,
    [FromQuery] List<long> fightResultsIds,
    CancellationToken cancellationToken)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToInvalidResult();
                return BadRequest(BaseModel<MessageModel>.Failed(errors));
            }

            var result = await championshipService.RecordFightResultsAsync(championshipId, fightResultsIds, cancellationToken);

            switch (result.Status)
            {
                case ResultStatus.Invalid:
                    return BadRequest(result.ValidationErrors);
                case ResultStatus.NotFound:
                    return NotFound(result.ValidationErrors);
                case ResultStatus.Error:
                    return StatusCode(StatusCodes.Status500InternalServerError);
                default:
                    return Created("Championship", BaseModel<MessageModel>.Success(new MessageModel("Resultados das " +
                        "lutas registrados com sucesso")));
            }
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [Authorize(Roles = RoleNames.Administrator + "," + RoleNames.User)]
    [HttpGet("{championshipId:long}/results/export")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseModel<MessageModel>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseModel<MessageModel>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseModel<MessageModel>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ExportResults(
     [FromServices] IChampionshipService championshipService,
     [FromRoute] long championshipId,
     CancellationToken cancellationToken)
    {
        try
        {
            var fileDataResult = await championshipService.ExportResultsAsync(championshipId, cancellationToken);

            if (fileDataResult.Status == ResultStatus.NotFound) return EntityNotFound(fileDataResult.ValidationErrors);

            if (fileDataResult.Status == ResultStatus.Error) return InternalServerError();

            if (fileDataResult.Status == ResultStatus.Invalid) return BadRequest(fileDataResult.ValidationErrors);

            var fileData = fileDataResult.Value;
            return File(fileData.Content, fileData.ContentType, fileData.FileName);
        }
        catch (Exception)
        {
            return InternalServerError();
        }
    }
}