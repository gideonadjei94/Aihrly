using Aihrly.Api.Common;
using Aihrly.Api.Data;
using Aihrly.Api.Domain;
using Aihrly.Api.DTOs.Requests;
using Aihrly.Api.DTOs.Responses;
using Aihrly.Api.Entities;
using Aihrly.Api.Enums;
using Aihrly.Api.Exceptions;
using Aihrly.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Aihrly.Api.Services;

public class ApplicationService(AppDbContext db) : IApplicationService
{
    public async Task<CreatedResponse> CreateAsync(Guid jobId, CreateApplicationRequest request)
    {
        var jobExists = await db.Jobs.AnyAsync(j => j.Id == jobId);
        if (!jobExists)
            throw new NotFoundException(nameof(Job), jobId);

        // Checking If a candidate with the same email has already applied to this job (case-insensitive)
        var alreadyApplied = await db.Applications
            .AnyAsync(a => a.JobId == jobId && a.CandidateEmail == request.CandidateEmail.ToLower());

        if (alreadyApplied)
            throw new ConflictException($"A candidate with email '{request.CandidateEmail}' has already applied to this job.");

        var application = new Application
        {
            Id = Guid.NewGuid(),
            JobId = jobId,
            CandidateName  = request.CandidateName,
            CandidateEmail = request.CandidateEmail.ToLower(),
            CoverLetter = request.CoverLetter,
            Stage = ApplicationStage.Applied,
            AppliedAt = DateTime.UtcNow
        };

        db.Applications.Add(application);
        await db.SaveChangesAsync();

        return new CreatedResponse(application.Id);
    }

    public async Task<PagedResult<ApplicationSummaryResponse>> ListByJobAsync(
        Guid jobId, string? stage, int page, int pageSize)
    {
        var jobExists = await db.Jobs.AnyAsync(j => j.Id == jobId);
        if (!jobExists)
            throw new NotFoundException(nameof(Job), jobId);

        var query = db.Applications
            .AsNoTracking()
            .Where(a => a.JobId == jobId);

        if (!string.IsNullOrWhiteSpace(stage))
        {
            if (!EnumParser.TryParse<ApplicationStage>(stage, out var parsedStage))
                throw new BadRequestException($"Invalid stage '{stage}'. Must be one of: {string.Join(", ", Enum.GetNames<ApplicationStage>())}.");

            query = query.Where(a => a.Stage == parsedStage);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(a => a.AppliedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new ApplicationSummaryResponse(
                a.Id,
                a.JobId,
                a.CandidateName,
                a.CandidateEmail,
                a.Stage.ToString(),
                a.AppliedAt))
            .ToListAsync();

        return new PagedResult<ApplicationSummaryResponse>(items, totalCount, page, pageSize);
    }

    public async Task<ApplicationProfileResponse> GetProfileAsync(Guid id)
    {
        // Single query — EF loads the application with all related data via JOINs
        var application = await db.Applications
            .AsNoTracking()
            .Include(a => a.Job)
            .Include(a => a.Notes).ThenInclude(n => n.Author)
            .Include(a => a.Scores).ThenInclude(s => s.Scorer)
            .Include(a => a.StageHistory).ThenInclude(h => h.ChangedByMember)
            .FirstOrDefaultAsync(a => a.Id == id)
            ?? throw new NotFoundException(nameof(Application), id);

        var scores = application.Scores
            .Select(s => new ScoreResponse(
                s.Dimension.ToString(),
                s.Score,
                s.Comment,
                s.Scorer.Name,
                s.ScoredAt))
            .ToList();

        var notes = application.Notes
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new NoteResponse(
                n.Id,
                n.Type.ToString(),
                n.Description,
                n.Author.Name,
                n.CreatedAt))
            .ToList();

        var history = application.StageHistory
            .OrderBy(h => h.ChangedAt)
            .Select(h => new StageHistoryResponse(
                h.FromStage.ToString(),
                h.ToStage.ToString(),
                h.ChangedByMember.Name,
                h.ChangedAt,
                h.Reason))
            .ToList();

        return new ApplicationProfileResponse(
            application.Id,
            application.JobId,
            application.Job.Title,
            application.CandidateName,
            application.CandidateEmail,
            application.CoverLetter,
            application.Stage.ToString(),
            application.AppliedAt,
            scores,
            notes,
            history);
    }

    public async Task MoveStageAsync(Guid id, MoveStageRequest request, Guid teamMemberId)
    {
        var application = await db.Applications
            .FirstOrDefaultAsync(a => a.Id == id)
            ?? throw new NotFoundException(nameof(Application), id);

        // EnumParser handles "screening", "Screening", "SCREENING" ...
        EnumParser.TryParse<ApplicationStage>(request.Stage, out var targetStage);

        if (!StageTransitionRules.IsValid(application.Stage, targetStage))
        {
            var allowed = StageTransitionRules.AllowedFrom(application.Stage);
            var allowedStr = allowed.Any()
                ? string.Join(", ", allowed)
                : "none — this is a terminal stage";

            throw new BadRequestException(
                $"Cannot move from '{application.Stage}' to '{targetStage}'. " +
                $"Allowed transitions: {allowedStr}.");
        }

        var history = new StageHistory
        {
            Id = Guid.NewGuid(),
            ApplicationId = application.Id,
            FromStage = application.Stage,
            ToStage = targetStage,
            ChangedBy = teamMemberId,
            ChangedAt = DateTime.UtcNow,
            Reason = request.Reason
        };

        application.Stage = targetStage;

        db.StageHistories.Add(history);
        await db.SaveChangesAsync();
    }
}
