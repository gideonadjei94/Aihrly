using Aihrly.Api.DTOs.Requests;
using Aihrly.Api.DTOs.Responses;

namespace Aihrly.Api.Services.Interfaces;

public interface INoteService
{
    Task<CreatedResponse> AddAsync(Guid applicationId, AddNoteRequest request, Guid teamMemberId);
    Task<IReadOnlyList<NoteResponse>> ListAsync(Guid applicationId);
}
