using Aihrly.Api.Common;
using Aihrly.Api.DTOs.Requests;
using Aihrly.Api.Enums;
using Aihrly.Api.Filters;
using Aihrly.Api.Services.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Aihrly.Api.Controllers;

[ApiController]
public class ApplicationsController(
    IApplicationService applicationService,
    INoteService noteService,
    IScoreService scoreService,
    IValidator<CreateApplicationRequest> createApplicationValidator,
    IValidator<MoveStageRequest> moveStageValidator,
    IValidator<AddNoteRequest> addNoteValidator,
    IValidator<ScoreRequest> scoreValidator) : ControllerBase
{
    // ── Applications Endpints ─────────────────────────────────────────────────────────────────

    // Public endpoint — no X-Team-Member-Id required
    [HttpPost("api/jobs/{jobId:guid}/applications")]
    public async Task<IActionResult> Create(Guid jobId, [FromBody] CreateApplicationRequest request)
    {
        var validation = await createApplicationValidator.ValidateAsync(request);
        if (!validation.IsValid)
            return BadRequest(validation.ToValidationProblemDetails());

        var result = await applicationService.CreateAsync(jobId, request);
        return CreatedAtAction(nameof(GetProfile), new { id = result.Id }, result);
    }

    [HttpGet("api/jobs/{jobId:guid}/applications")]
    public async Task<IActionResult> ListByJob(
        Guid jobId,
        [FromQuery] string? stage,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (page < 1)
            return BadRequest(Problem("Page must be 1 or greater.", statusCode: 400));

        if (pageSize is < 1 or > 100)
            return BadRequest(Problem("Page size must be between 1 and 100.", statusCode: 400));

        var result = await applicationService.ListByJobAsync(jobId, stage, page, pageSize);
        return Ok(result);
    }

    [HttpGet("api/applications/{id:guid}")]
    public async Task<IActionResult> GetProfile(Guid id)
    {
        var result = await applicationService.GetProfileAsync(id);
        return Ok(result);
    }

    // Requires a valid team member to be identified via header
    [HttpPatch("api/applications/{id:guid}/stage")]
    [RequireTeamMember]
    public async Task<IActionResult> MoveStage(Guid id, [FromBody] MoveStageRequest request)
    {
        var validation = await moveStageValidator.ValidateAsync(request);
        if (!validation.IsValid)
            return BadRequest(validation.ToValidationProblemDetails());

        var teamMemberId = (Guid)HttpContext.Items[RequireTeamMemberAttribute.ContextKey]!;
        await applicationService.MoveStageAsync(id, request, teamMemberId);
        return Ok(new { message = "Application stage updated successfully." });
    }

    // ── Notes Endpints ─────────────────────────────────────────────────────────────────

    [HttpPost("api/applications/{id:guid}/notes")]
    [RequireTeamMember]
    public async Task<IActionResult> AddNote(Guid id, [FromBody] AddNoteRequest request)
    {
        var validation = await addNoteValidator.ValidateAsync(request);
        if (!validation.IsValid)
            return BadRequest(validation.ToValidationProblemDetails());

        var teamMemberId = (Guid)HttpContext.Items[RequireTeamMemberAttribute.ContextKey]!;
        var result = await noteService.AddAsync(id, request, teamMemberId);
        return Created($"api/applications/{id}/notes/{result.Id}", result);
    }

    [HttpGet("api/applications/{id:guid}/notes")]
    public async Task<IActionResult> ListNotes(Guid id)
    {
        var result = await noteService.ListAsync(id);
        return Ok(result);
    }

    // ── Scores Endpoints ────────────────────────────────────────────────────────────────

    [HttpPut("api/applications/{id:guid}/scores/culture-fit")]
    [RequireTeamMember]
    public Task<IActionResult> ScoreCultureFit(Guid id, [FromBody] ScoreRequest request) =>
        UpsertScore(id, ScoreDimension.CultureFit, request);

    [HttpPut("api/applications/{id:guid}/scores/interview")]
    [RequireTeamMember]
    public Task<IActionResult> ScoreInterview(Guid id, [FromBody] ScoreRequest request) =>
        UpsertScore(id, ScoreDimension.Interview, request);

    [HttpPut("api/applications/{id:guid}/scores/assessment")]
    [RequireTeamMember]
    public Task<IActionResult> ScoreAssessment(Guid id, [FromBody] ScoreRequest request) =>
        UpsertScore(id, ScoreDimension.Assessment, request);

    private async Task<IActionResult> UpsertScore(Guid applicationId, ScoreDimension dimension, ScoreRequest request)
    {
        var validation = await scoreValidator.ValidateAsync(request);
        if (!validation.IsValid)
            return BadRequest(validation.ToValidationProblemDetails());

        var teamMemberId = (Guid)HttpContext.Items[RequireTeamMemberAttribute.ContextKey]!;
        await scoreService.UpsertAsync(applicationId, dimension, request, teamMemberId);
        return NoContent();
    }
}
