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

public class NoteService(AppDbContext db) : INoteService
{
    public async Task<CreatedResponse> AddAsync(Guid applicationId, AddNoteRequest request, Guid teamMemberId)
    {
        var applicationExists = await db.Applications.AnyAsync(a => a.Id == applicationId);
        if (!applicationExists)
            throw new NotFoundException(nameof(Application), applicationId);

        EnumParser.TryParse<NoteType>(request.Type, out var noteType);

        var note = new ApplicationNote
        {
            Id = Guid.NewGuid(),
            ApplicationId = applicationId,
            Type = noteType,
            Description = request.Description,
            CreatedBy = teamMemberId,
            CreatedAt = DateTime.UtcNow
        };

        db.ApplicationNotes.Add(note);
        await db.SaveChangesAsync();

        return new CreatedResponse(note.Id);
    }

    public async Task<IReadOnlyList<NoteResponse>> ListAsync(Guid applicationId)
    {
        var applicationExists = await db.Applications.AnyAsync(a => a.Id == applicationId);
        if (!applicationExists)
            throw new NotFoundException(nameof(Application), applicationId);

        var notes = await db.ApplicationNotes
            .AsNoTracking()
            .Include(n => n.Author)
            .Where(n => n.ApplicationId == applicationId)
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new NoteResponse(
                n.Id,
                n.Type.ToString(),
                n.Description,
                n.Author.Name,
                n.CreatedAt))
            .ToListAsync();

        return notes;
    }
}
