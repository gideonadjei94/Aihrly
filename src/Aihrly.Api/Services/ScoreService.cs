using Aihrly.Api.Data;
using Aihrly.Api.DTOs.Requests;
using Aihrly.Api.Entities;
using Aihrly.Api.Enums;
using Aihrly.Api.Exceptions;
using Aihrly.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Aihrly.Api.Services;

public class ScoreService(AppDbContext db) : IScoreService
{
    public async Task UpsertAsync(Guid applicationId, ScoreDimension dimension, ScoreRequest request, Guid teamMemberId)
    {
        var applicationExists = await db.Applications.AnyAsync(a => a.Id == applicationId);
        if (!applicationExists)
            throw new NotFoundException(nameof(Application), applicationId);

        // Check if a score for this dimension already exists
        var existing = await db.ApplicationScores
            .FirstOrDefaultAsync(s => s.ApplicationId == applicationId && s.Dimension == dimension);

        if (existing is not null)
        {
            // Overwrite — second submission wins, track who changed it and when
            existing.Score    = request.Score;
            existing.Comment  = request.Comment;
            existing.ScoredBy = teamMemberId;
            existing.ScoredAt = DateTime.UtcNow;
        }
        else
        {
            var score = new ApplicationScore
            {
                Id            = Guid.NewGuid(),
                ApplicationId = applicationId,
                Dimension     = dimension,
                Score         = request.Score,
                Comment       = request.Comment,
                ScoredBy      = teamMemberId,
                ScoredAt      = DateTime.UtcNow
            };

            db.ApplicationScores.Add(score);
        }

        await db.SaveChangesAsync();
    }
}
