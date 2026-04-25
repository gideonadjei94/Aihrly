using Aihrly.Api.DTOs.Requests;
using Aihrly.Api.Enums;

namespace Aihrly.Api.Services.Interfaces;

public interface IScoreService
{
    Task UpsertAsync(Guid applicationId, ScoreDimension dimension, ScoreRequest request, Guid teamMemberId);
}
