using Ardalis.Result;
using AutoMapper;
using CommonUtility.Model;
using JiuJitsuMaster.AdminApi.Controllers;
using JiuJitsuMaster.AdminAPI.Models.Athlete;
using JiuJitsuMaster.Core.Common;
using JiuJitsuMaster.Core.Enums;
using JiuJitsuMaster.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JiuJitsuMaster.AdminAPI.Controllers;

[Route("[controller]")]
public class AthleteController : JiuJtsuControllerBase
{
    [Authorize(Roles = RoleNames.Administrator + "," + RoleNames.User)]
    [HttpGet("Athletes")]
    [ProducesResponseType(typeof(BaseModel<ListBaseModel<List<AthleteListResponseModel>>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseModel<ListBaseModel<List<AthleteListResponseModel>>>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAthleteRegistrations(
        [FromServices] IMapper mapper,
        [FromServices] IAthleteService athleteService,
        [FromQuery] int page,
        [FromQuery] int pageSize,
        [FromQuery] string? athleteName,
        [FromQuery] Gender? gender,
        [FromQuery] string? championshipTitle,
        [FromQuery] string? city,
        [FromQuery] string? state,
        CancellationToken cancellationToken)
    {
        if (page <= 0 || pageSize <= 0)
            return BadRequest("Página ou limite inválido");

        var result = await athleteService.GetAthleteRegistrationsAsync(
            page, pageSize, athleteName, gender, championshipTitle, city, state, cancellationToken);

        if (result.Status == ResultStatus.Invalid)
            return BadRequest(result.ValidationErrors);

        var listResponse = mapper.Map<List<AthleteListResponseModel>>(result.Value);

        var responseModel = new ListBaseModel<List<AthleteListResponseModel>>(result.Value.TotalCount, listResponse, page, pageSize);

        return Ok(BaseModel<ListBaseModel<List<AthleteListResponseModel>>>.Success(responseModel));
    }

    [Authorize(Roles = RoleNames.Administrator)]
    [HttpGet("athletes/export/csv")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExportAthleteRegistrationsToCsv(
        [FromServices] IAthleteService athleteService,
        [FromQuery] string? athleteName,
        [FromQuery] Gender? gender,
        [FromQuery] string? championshipTitle,
        [FromQuery] string? city,
        [FromQuery] string? state,
        CancellationToken cancellationToken)
    {
        var fileData = await athleteService.ExportAthleteRegistrationsToCsvAsync(
            athleteName, gender, championshipTitle, city, state, cancellationToken);

        if (fileData.Status == ResultStatus.NotFound) return EntityNotFound(fileData.ValidationErrors);

        return File(fileData.Value.Content, fileData.Value.ContentType, fileData.Value.FileName);
    }

    [Authorize(Roles = RoleNames.Administrator)]
    [HttpGet("athletes/export/pdf")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExportAthleteRegistrationsToPdf(
        [FromServices] IAthleteService athleteService,
        [FromQuery] string? athleteName,
        [FromQuery] Gender? gender,
        [FromQuery] string? championshipTitle,
        [FromQuery] string? city,
        [FromQuery] string? state,
        CancellationToken cancellationToken)
    {
        var fileData = await athleteService.ExportAthleteRegistrationsToPdfAsync(
            athleteName, gender, championshipTitle, city, state, cancellationToken);

        if (fileData.Status == ResultStatus.NotFound) return EntityNotFound(fileData.ValidationErrors);

        return File(fileData.Value.Content, fileData.Value.ContentType, fileData.Value.FileName);
    }
}