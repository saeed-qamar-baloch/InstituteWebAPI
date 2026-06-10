using InstituteWebAPI.Helpers;
using InstituteWebApp.Models.Domain;
using Xunit;

namespace InstituteWebAPI.Tests;

/// <summary>
/// Unit tests for <see cref="FeeCalculator"/>.
/// No database, no DI — pure function tests only.
/// </summary>
public class FeeCalculatorTests
{
    // ── NormalizeAmount ──────────────────────────────────────────────────────

    [Theory]
    [InlineData(0,     0)]
    [InlineData(100,   100)]
    [InlineData(50.5,  50.5)]
    public void NormalizeAmount_PositiveOrZero_ReturnsSameValue(decimal input, decimal expected)
    {
        Assert.Equal(expected, FeeCalculator.NormalizeAmount(input));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-0.01)]
    [InlineData(-999)]
    public void NormalizeAmount_Negative_ReturnsZero(decimal input)
    {
        Assert.Equal(0m, FeeCalculator.NormalizeAmount(input));
    }

    // ── ApplyLateFeeIfNeeded ─────────────────────────────────────────────────

    [Fact]
    public void ApplyLateFeeIfNeeded_MonthlyDueOverdue_SetsLateFeeAndReturnsTrue()
    {
        var due = MakeMonthlyDue(dueDate: new DateTime(2025, 1, 10));
        var today = new DateTime(2025, 1, 15);

        var result = FeeCalculator.ApplyLateFeeIfNeeded(due, today, lateFeeAmount: 50m);

        Assert.True(result);
        Assert.Equal(50m, due.LateFeeAmount);
    }

    [Fact]
    public void ApplyLateFeeIfNeeded_DueDateIsToday_DoesNotApplyLateFee()
    {
        var today = new DateTime(2025, 2, 5);
        var due = MakeMonthlyDue(dueDate: today);

        var result = FeeCalculator.ApplyLateFeeIfNeeded(due, today, lateFeeAmount: 50m);

        Assert.False(result);
        Assert.Equal(0m, due.LateFeeAmount);
    }

    [Fact]
    public void ApplyLateFeeIfNeeded_DueDateInFuture_DoesNotApplyLateFee()
    {
        var today = new DateTime(2025, 2, 3);
        var due = MakeMonthlyDue(dueDate: new DateTime(2025, 2, 10));

        var result = FeeCalculator.ApplyLateFeeIfNeeded(due, today, lateFeeAmount: 50m);

        Assert.False(result);
        Assert.Equal(0m, due.LateFeeAmount);
    }

    [Fact]
    public void ApplyLateFeeIfNeeded_FeeAlreadyWaived_DoesNotChangeAnything()
    {
        var due = MakeMonthlyDue(dueDate: new DateTime(2025, 1, 1));
        due.IsLateFeeWaived = true;
        var today = new DateTime(2025, 2, 1);

        var result = FeeCalculator.ApplyLateFeeIfNeeded(due, today, lateFeeAmount: 50m);

        Assert.False(result);
        Assert.Equal(0m, due.LateFeeAmount);
    }

    [Fact]
    public void ApplyLateFeeIfNeeded_LateFeeAlreadySet_DoesNotOverwrite()
    {
        var due = MakeMonthlyDue(dueDate: new DateTime(2025, 1, 1));
        due.LateFeeAmount = 30m; // already applied
        var today = new DateTime(2025, 2, 1);

        var result = FeeCalculator.ApplyLateFeeIfNeeded(due, today, lateFeeAmount: 50m);

        Assert.False(result);
        Assert.Equal(30m, due.LateFeeAmount); // unchanged
    }

    [Theory]
    [InlineData(FeeDueType.Admission)]
    [InlineData(FeeDueType.Card)]
    public void ApplyLateFeeIfNeeded_NonMonthlyDue_DoesNotApplyLateFee(FeeDueType feeType)
    {
        var due = new FeeDue
        {
            FeeType = feeType,
            DueDate = new DateTime(2025, 1, 1),
            LateFeeAmount = 0m,
            IsLateFeeWaived = false
        };
        var today = new DateTime(2025, 2, 1);

        var result = FeeCalculator.ApplyLateFeeIfNeeded(due, today, lateFeeAmount: 50m);

        Assert.False(result);
        Assert.Equal(0m, due.LateFeeAmount);
    }

    [Fact]
    public void ApplyLateFeeIfNeeded_NegativeLateFeeConfigured_StoresZeroNotNegative()
    {
        var due = MakeMonthlyDue(dueDate: new DateTime(2025, 1, 1));
        var today = new DateTime(2025, 2, 1);

        FeeCalculator.ApplyLateFeeIfNeeded(due, today, lateFeeAmount: -10m);

        // NormalizeAmount clamps negative amounts to 0
        Assert.Equal(0m, due.LateFeeAmount);
    }

    // ── ApplyConcession ──────────────────────────────────────────────────────

    [Fact]
    public void ApplyConcession_NoDiscount_ReturnsFullAmountUnpaidWithLateFee()
    {
        var (base_, status, applyLate) = FeeCalculator.ApplyConcession(1000m, discountPercent: 0m);

        Assert.Equal(1000m, base_);
        Assert.Equal(FeeDueStatus.Unpaid, status);
        Assert.True(applyLate);
    }

    [Fact]
    public void ApplyConcession_HundredPercent_ReturnsZeroWaivedNoLateFee()
    {
        var (base_, status, applyLate) = FeeCalculator.ApplyConcession(1000m, discountPercent: 100m);

        Assert.Equal(0m, base_);
        Assert.Equal(FeeDueStatus.Waived, status);
        Assert.False(applyLate);
    }

    [Fact]
    public void ApplyConcession_FiftyPercent_ReturnsHalfAmountUnpaidNoLateFee()
    {
        var (base_, status, applyLate) = FeeCalculator.ApplyConcession(1000m, discountPercent: 50m);

        Assert.Equal(500m, base_);
        Assert.Equal(FeeDueStatus.Unpaid, status);
        Assert.False(applyLate); // partial scholarship → no late fee
    }

    [Fact]
    public void ApplyConcession_TwentyFivePercent_ReducesByCorrectAmount()
    {
        var (base_, status, applyLate) = FeeCalculator.ApplyConcession(800m, discountPercent: 25m);

        Assert.Equal(600m, base_);   // 800 * 75% = 600
        Assert.Equal(FeeDueStatus.Unpaid, status);
        Assert.False(applyLate);
    }

    [Fact]
    public void ApplyConcession_DiscountAbove100_ClampsToFullWaiver()
    {
        // A stored discount > 100 should behave as 100 (guard against bad data)
        var (base_, status, _) = FeeCalculator.ApplyConcession(500m, discountPercent: 150m);

        Assert.Equal(0m, base_);
        Assert.Equal(FeeDueStatus.Waived, status);
    }

    [Fact]
    public void ApplyConcession_DiscountNegative_TreatedAsNoDiscount()
    {
        // Math.Clamp(neg, 0, 100) = 0
        var (base_, status, applyLate) = FeeCalculator.ApplyConcession(500m, discountPercent: -10m);

        Assert.Equal(500m, base_);
        Assert.Equal(FeeDueStatus.Unpaid, status);
        Assert.True(applyLate);
    }

    // ── ComputeLabelMonth (25th-rule) ────────────────────────────────────────

    [Theory]
    [InlineData(1,  2025, 1,  2025, 1)]  // day 1  → same month
    [InlineData(24, 2025, 3,  2025, 3)]  // day 24 → same month
    [InlineData(25, 2025, 3,  2025, 4)]  // day 25 → next month (25th-rule)
    [InlineData(31, 2025, 1,  2025, 2)]  // day 31 → next month
    public void ComputeLabelMonth_25thRule_IsCorrect(
        int dueDay, int collYear, int collMonth, int expectedYear, int expectedMonth)
    {
        var collectionMonth = new DateTime(collYear, collMonth, 1);
        var label = FeeCalculator.ComputeLabelMonth(collectionMonth, dueDay);

        Assert.Equal(new DateTime(expectedYear, expectedMonth, 1), label);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static FeeDue MakeMonthlyDue(DateTime dueDate) => new()
    {
        FeeType = FeeDueType.Monthly,
        DueDate = dueDate,
        LateFeeAmount = 0m,
        IsLateFeeWaived = false
    };
}
