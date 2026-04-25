using Aihrly.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace Aihrly.Api.Filters;


[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireTeamMemberAttribute : Attribute, IAsyncActionFilter
{
    public const string HeaderName = "X-Team-Member-Id";
    public const string ContextKey = "TeamMemberId";

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue(HeaderName, out var rawValue)
            || !Guid.TryParse(rawValue, out var teamMemberId))
        {
            context.Result = new ObjectResult(new ProblemDetails
            {
                Title  = "Missing or invalid header",
                Detail = $"'{HeaderName}' header must be a valid GUID.",
                Status = StatusCodes.Status400BadRequest
            })
            { StatusCode = StatusCodes.Status400BadRequest };

            return;
        }

        // Confirming the team member actually exists in the database
        var db = context.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
        var exists = await db.TeamMembers.AnyAsync(m => m.Id == teamMemberId);

        if (!exists)
        {
            context.Result = new ObjectResult(new ProblemDetails
            {
                Title  = "Team member not found",
                Detail = $"No team member exists with id '{teamMemberId}'.",
                Status = StatusCodes.Status400BadRequest
            })
            { StatusCode = StatusCodes.Status400BadRequest };

            return;
        }

        // Storing the validated GUID so services can use it without re-parsing the header
        context.HttpContext.Items[ContextKey] = teamMemberId;

        await next();
    }
}
