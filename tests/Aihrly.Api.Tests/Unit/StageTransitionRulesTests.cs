using Aihrly.Api.Domain;
using Xunit;
using Aihrly.Api.Enums;

namespace Aihrly.Api.Tests.Unit;

public class StageTransitionRulesTests
{
    // --- Valid transitions ---

    [Theory]
    [InlineData(ApplicationStage.Applied, ApplicationStage.Screening)]
    [InlineData(ApplicationStage.Applied, ApplicationStage.Rejected)]
    [InlineData(ApplicationStage.Screening, ApplicationStage.Interview)]
    [InlineData(ApplicationStage.Screening, ApplicationStage.Rejected)]
    [InlineData(ApplicationStage.Interview, ApplicationStage.Offer)]
    [InlineData(ApplicationStage.Interview, ApplicationStage.Rejected)]
    [InlineData(ApplicationStage.Offer, ApplicationStage.Hired)]
    [InlineData(ApplicationStage.Offer, ApplicationStage.Rejected)]
    public void IsValid_ReturnsTrue_ForAllLegalTransitions(ApplicationStage from, ApplicationStage to)
    {
        var result = StageTransitionRules.IsValid(from, to);
        Assert.True(result);
    }

    // --- Invalid transitions ---

    [Theory]
    [InlineData(ApplicationStage.Applied,   ApplicationStage.Interview)] // skipping stages
    [InlineData(ApplicationStage.Applied,   ApplicationStage.Offer)]
    [InlineData(ApplicationStage.Applied,   ApplicationStage.Hired)]
    [InlineData(ApplicationStage.Screening, ApplicationStage.Offer)]
    [InlineData(ApplicationStage.Screening, ApplicationStage.Hired)]
    [InlineData(ApplicationStage.Interview, ApplicationStage.Hired)]
    [InlineData(ApplicationStage.Hired,     ApplicationStage.Rejected)]  // terminal — no moves allowed
    [InlineData(ApplicationStage.Rejected,  ApplicationStage.Screening)] // terminal — no moves allowed
    [InlineData(ApplicationStage.Applied,   ApplicationStage.Applied)]   // same stage is not a transition
    public void IsValid_ReturnsFalse_ForAllIllegalTransitions(ApplicationStage from, ApplicationStage to)
    {
        var result = StageTransitionRules.IsValid(from, to);
        Assert.False(result);
    }

    // --- Terminal stage detection ---

    [Theory]
    [InlineData(ApplicationStage.Hired)]
    [InlineData(ApplicationStage.Rejected)]
    public void IsTerminal_ReturnsTrue_ForTerminalStages(ApplicationStage stage)
    {
        Assert.True(StageTransitionRules.IsTerminal(stage));
    }

    [Theory]
    [InlineData(ApplicationStage.Applied)]
    [InlineData(ApplicationStage.Screening)]
    [InlineData(ApplicationStage.Interview)]
    [InlineData(ApplicationStage.Offer)]
    public void IsTerminal_ReturnsFalse_ForNonTerminalStages(ApplicationStage stage)
    {
        Assert.False(StageTransitionRules.IsTerminal(stage));
    }

    // --- AllowedFrom ---

    [Fact]
    public void AllowedFrom_ReturnsEmpty_ForTerminalStages()
    {
        Assert.Empty(StageTransitionRules.AllowedFrom(ApplicationStage.Hired));
        Assert.Empty(StageTransitionRules.AllowedFrom(ApplicationStage.Rejected));
    }

    [Fact]
    public void AllowedFrom_Applied_ReturnsTwoOptions()
    {
        var allowed = StageTransitionRules.AllowedFrom(ApplicationStage.Applied);
        Assert.Contains(ApplicationStage.Screening, allowed);
        Assert.Contains(ApplicationStage.Rejected, allowed);
        Assert.Equal(2, allowed.Count);
    }
}
