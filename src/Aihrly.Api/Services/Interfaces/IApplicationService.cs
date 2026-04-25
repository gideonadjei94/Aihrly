using Aihrly.Api.Common;
using Aihrly.Api.DTOs.Requests;
using Aihrly.Api.DTOs.Responses;

namespace Aihrly.Api.Services.Interfaces;

public interface IApplicationService
{
    Task<CreatedResponse> CreateAsync(Guid jobId, CreateApplicationRequest request);
    Task<PagedResult<ApplicationSummaryResponse>> ListByJobAsync(Guid jobId, string? stage, int page, int pageSize);
    Task<ApplicationProfileResponse> GetProfileAsync(Guid id);
    Task MoveStageAsync(Guid id, MoveStageRequest request, Guid teamMemberId);
}
