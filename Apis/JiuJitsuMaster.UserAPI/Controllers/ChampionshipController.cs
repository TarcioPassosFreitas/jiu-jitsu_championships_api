using Ardalis.Result;
using AutoMapper;
using CommonUtility.Model;
using JiuJitsuMaster.Core.Enums;
using JiuJitsuMaster.Core.Interfaces.Services;
using JiuJitsuMaster.UserAPI.Models.Championship;
using Microsoft.AspNetCore.Mvc;

namespace JiuJitsuMaster.UserAPI.Controllers;

[Route("[controller]")]
public class ChampionshipController : JiuJtsuControllerBase
{
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

    [HttpGet("championships/regular")]
    [ProducesResponseType(typeof(BaseModel<ListBaseModel<List<ChampionshipResponseModel>>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseModel<ListBaseModel<List<ChampionshipResponseModel>>>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRegularChampionships(
        [FromServices] IMapper mapper,
        [FromServices] IChampionshipService championshipService,
        [FromQuery] string? state,
        [FromQuery] string? city,
        [FromQuery] ChampionshipType? type,
        [FromQuery] string? title,
       CancellationToken cancellationToken)
    {
        try
        {
            var result = await championshipService.GetNonHighlightedAsync(1, 8, state, city, type, title, cancellationToken);

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

    [HttpGet("{id:long}/FightKeys")]
    [ProducesResponseType(typeof(BaseModel<ListBaseModel<List<ChampionshipResponseModel>>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseModel<ListBaseModel<List<ChampionshipResponseModel>>>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFightKeys(
        [FromServices] IMapper mapper,
        [FromServices] IChampionshipService championshipService,
        [FromRoute] long id,
       CancellationToken cancellationToken)
    {
        try
        {
            var result = await championshipService.GetFightKeysAsync(id, cancellationToken);

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
}