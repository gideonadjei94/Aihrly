using Aihrly.Api.Enums;

namespace Aihrly.Api.Domain;

public static class StageTransitionRules
{
    private static readonly Dictionary<ApplicationStage, ApplicationStage[]> ValidTransitions = new()
    {
        [ApplicationStage.Applied] = [ApplicationStage.Screening, ApplicationStage.Rejected],
        [ApplicationStage.Screening] = [ApplicationStage.Interview, ApplicationStage.Rejected],
        [ApplicationStage.Interview] = [ApplicationStage.Offer, ApplicationStage.Rejected],
        [ApplicationStage.Offer] = [ApplicationStage.Hired, ApplicationStage.Rejected],
        [ApplicationStage.Hired] = [],
        [ApplicationStage.Rejected] = []
    };

    public static bool IsValid(ApplicationStage from, ApplicationStage to) =>
        ValidTransitions.TryGetValue(from, out var allowed) && allowed.Contains(to);

    public static bool IsTerminal(ApplicationStage stage) =>
        stage is ApplicationStage.Hired or ApplicationStage.Rejected;

    public static IReadOnlyList<ApplicationStage> AllowedFrom(ApplicationStage from) =>
        ValidTransitions.TryGetValue(from, out var allowed) ? allowed : [];
}
