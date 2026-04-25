using Aihrly.Api.Common;
using Aihrly.Api.Data;
using Aihrly.Api.DTOs.Requests;
using Aihrly.Api.DTOs.Responses;
using Aihrly.Api.Entities;
using Aihrly.Api.Enums;
using Aihrly.Api.Exceptions;
using Aihrly.Api.Domain;
using Aihrly.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Aihrly.Api.Services;

public class JobService(AppDbContext db) : IJobService
{
    public async Task<CreatedResponse> CreateAsync(CreateJobRequest request)
    {
        var job = new Job
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            Location = request.Location,
            Status = JobStatus.Open,
            CreatedAt = DateTime.UtcNow
        };

        db.Jobs.Add(job);
        await db.SaveChangesAsync();

        return new CreatedResponse(job.Id);
    }

    public async Task<PagedResult<JobResponse>> ListAsync(string? status, int page, int pageSize)
    {
        var query = db.Jobs.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!EnumParser.TryParse<JobStatus>(status, out var parsedStatus))
                throw new BadRequestException($"Invalid status '{status}'. Must be one of: {string.Join(", ", Enum.GetNames<JobStatus>())}.");

            query = query.Where(j => j.Status == parsedStatus);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(j => j.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(j => ToResponse(j))
            .ToListAsync();

        return new PagedResult<JobResponse>(items, totalCount, page, pageSize);
    }

    public async Task<JobResponse> GetByIdAsync(Guid id)
    {
        var job = await db.Jobs
            .AsNoTracking()
            .FirstOrDefaultAsync(j => j.Id == id)
            ?? throw new NotFoundException(nameof(Job), id);

        return ToResponse(job);
    }

    private static JobResponse ToResponse(Job job) =>
        new(job.Id, job.Title, job.Description, job.Location, job.Status.ToString(), job.CreatedAt);
}
