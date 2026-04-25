using Aihrly.Api.Common;
using Aihrly.Api.DTOs.Requests;
using Aihrly.Api.Services.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Aihrly.Api.Controllers;

[ApiController]
[Route("api/jobs")]
public class JobsController(IJobService jobService, IValidator<CreateJobRequest> validator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateJobRequest request)
    {
        var validation = await validator.ValidateAsync(request);
        if (!validation.IsValid)
            return BadRequest(validation.ToValidationProblemDetails());

        var result = await jobService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (page < 1)
            return BadRequest(Problem("Page must be 1 or greater.", statusCode: 400));

        if (pageSize is < 1 or > 100)
            return BadRequest(Problem("Page size must be between 1 and 100.", statusCode: 400));

        var result = await jobService.ListAsync(status, page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await jobService.GetByIdAsync(id);
        return Ok(result);
    }
}
