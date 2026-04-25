using Aihrly.Api.Common;
using Aihrly.Api.DTOs.Requests;
using Aihrly.Api.DTOs.Responses;

namespace Aihrly.Api.Services.Interfaces;

public interface IJobService
{
    Task<CreatedResponse> CreateAsync(CreateJobRequest request);
    Task<PagedResult<JobResponse>> ListAsync(string? status, int page, int pageSize);
    Task<JobResponse> GetByIdAsync(Guid id);
}
