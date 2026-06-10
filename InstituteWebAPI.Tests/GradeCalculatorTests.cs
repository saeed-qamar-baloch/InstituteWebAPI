using InstituteWebAPI.Helpers;
using InstituteWebApp.Models.Domain;
using Xunit;

namespace InstituteWebAPI.Tests;

/// <summary>
/// Unit tests for <see cref="GradeCalculator"/>.
/// Covers both the default thresholds and DB-configured criteria.
/// </summary>
public class GradeCalculatorTests
{
    // ── Default grade thresholds ─────────────────────────────────────────────
    // A+=80, A=70, B=60, C=50, D=45, E=33, F=below 33

    [Theory]
    [InlineData(100f, "A+")]
    [InlineData(80f,  "A+")]
    [InlineData(79.9f,"A")]
    [InlineData(70f,  "A")]
    [InlineData(69.9f,"B")]
    [InlineData(60f,  "B")]
    [InlineData(59.9f,"C")]
    [InlineData(50f,  "C")]
    [InlineData(49.9f,"D")]
    [InlineData(45f,  "D")]
    [InlineData(44.9f,"E")]
    [InlineData(33f,  "E")]
    [InlineData(32.9f,"F")]
    [InlineData(0f,   "F")]
    public void ResolveFromDefaults_ReturnsCorrectGrade(float percentage, string expectedGrade)
    {
        var grade = GradeCalculator.ResolveFromDefaults(percentage);
        Assert.Equal(expectedGrade, grade);
    }

    // ── Null / empty criteria → falls back to defaults ───────────────────────

    [Fact]
    public void Resolve_NullCriteria_FallsBackToDefaults()
    {
        Assert.Equal("A+", GradeCalculator.Resolve(85f, null));
        Assert.Equal("F",  GradeCalculator.Resolve(10f, null));
    }

    [Fact]
    public void Resolve_EmptyCriteria_FallsBackToDefaults()
    {
        var grade = GradeCalculator.Resolve(75f, Enumerable.Empty<GradeCriteria>());
        Assert.Equal("A", grade);
    }

    // ── DB-configured criteria ───────────────────────────────────────────────

    [Fact]
    public void Resolve_WithDbCriteria_UsesDbThresholdsNotDefaults()
    {
        // A custom grading scale: Distinction ≥90, Merit ≥70, Pass ≥40, Fail <40
        var criteria = new List<GradeCriteria>
        {
            new() { GradeLabel = "Distinction", MinPercentage = 90f, DisplayOrder = 1 },
            new() { GradeLabel = "Merit",       MinPercentage = 70f, DisplayOrder = 2 },
            new() { GradeLabel = "Pass",        MinPercentage = 40f, DisplayOrder = 3 },
            new() { GradeLabel = "Fail",        MinPercentage = 0f,  DisplayOrder = 4 },
        };

        Assert.Equal("Distinction", GradeCalculator.Resolve(95f, criteria));
        Assert.Equal("Distinction", GradeCalculator.Resolve(90f, criteria));
        Assert.Equal("Merit",       GradeCalculator.Resolve(89f, criteria));
        Assert.Equal("Merit",       GradeCalculator.Resolve(70f, criteria));
        Assert.Equal("Pass",        GradeCalculator.Resolve(69f, criteria));
        Assert.Equal("Pass",        GradeCalculator.Resolve(40f, criteria));
        Assert.Equal("Fail",        GradeCalculator.Resolve(39f, criteria));
        Assert.Equal("Fail",        GradeCalculator.Resolve(0f,  criteria));
    }

    [Fact]
    public void Resolve_WithDbCriteria_CriteriaUnorderedInList_StillResolvesCorrectly()
    {
        // Deliberately pass criteria in wrong order — Resolve must sort them
        var criteria = new List<GradeCriteria>
        {
            new() { GradeLabel = "C", MinPercentage = 40f },
            new() { GradeLabel = "A", MinPercentage = 80f },
            new() { GradeLabel = "B", MinPercentage = 60f },
        };

        Assert.Equal("A", GradeCalculator.Resolve(85f, criteria));
        Assert.Equal("B", GradeCalculator.Resolve(65f, criteria));
        Assert.Equal("C", GradeCalculator.Resolve(45f, criteria));
        // Below all thresholds → lowest grade label (C, minPct=40)
        Assert.Equal("C", GradeCalculator.Resolve(20f, criteria));
    }

    [Fact]
    public void Resolve_ExactBoundary_IsInclusive()
    {
        var criteria = new List<GradeCriteria>
        {
            new() { GradeLabel = "Pass", MinPercentage = 50f },
            new() { GradeLabel = "Fail", MinPercentage = 0f  },
        };

        Assert.Equal("Pass", GradeCalculator.Resolve(50f,   criteria));
        Assert.Equal("Fail", GradeCalculator.Resolve(49.9f, criteria));
    }

    [Fact]
    public void Resolve_SingleCriterion_AlwaysReturnsThatLabel()
    {
        var criteria = new List<GradeCriteria>
        {
            new() { GradeLabel = "Only", MinPercentage = 0f }
        };

        Assert.Equal("Only", GradeCalculator.Resolve(0f,   criteria));
        Assert.Equal("Only", GradeCalculator.Resolve(100f, criteria));
    }
}
