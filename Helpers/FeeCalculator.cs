using InstituteWebApp.Models.Domain;

namespace InstituteWebAPI.Helpers
{
    /// <summary>
    /// Pure, stateless fee calculation functions extracted from FeeManagementService
    /// so they can be unit-tested without a database.
    /// </summary>
    public static class FeeCalculator
    {
        /// <summary>
        /// Clamps a fee amount to zero if it is negative.
        /// All fee amounts must pass through this before being stored or compared.
        /// </summary>
        public static decimal NormalizeAmount(decimal amount) =>
            amount < 0 ? 0m : amount;

        /// <summary>
        /// Returns true and sets <see cref="FeeDue.LateFeeAmount"/> when a late
        /// fee should be applied to the given due.
        ///
        /// Rules:
        ///  - Late fees apply only to <see cref="FeeDueType.Monthly"/> dues.
        ///  - No effect if the fee is already waived or already has a late fee.
        ///  - No effect if today is on or before the due date.
        /// </summary>
        public static bool ApplyLateFeeIfNeeded(FeeDue due, DateTime today, decimal lateFeeAmount)
        {
            if (due.FeeType != FeeDueType.Monthly)
                return false;

            if (due.IsLateFeeWaived || due.LateFeeAmount > 0m)
                return false;

            if (today <= due.DueDate.Date)
                return false;

            due.LateFeeAmount = NormalizeAmount(lateFeeAmount);
            return true;
        }

        /// <summary>
        /// Applies an active scholarship/leave to a base monthly fee amount.
        ///
        /// Returns:
        ///  - <c>baseAmount</c>  — the (possibly discounted) fee to charge
        ///  - <c>status</c>      — the initial <see cref="FeeDueStatus"/> for the new due
        ///  - <c>applyLateFee</c> — whether a late fee should be considered for this due
        /// </summary>
        /// <param name="discountPercent">
        ///   Percent discount from an active scholarship/leave (0–100).
        ///   Pass 0 if there is no active scholarship.
        /// </param>
        public static (decimal BaseAmount, FeeDueStatus Status, bool ApplyLateFee)
            ApplyConcession(decimal monthlyFee, decimal discountPercent)
        {
            if (discountPercent <= 0m)
                return (monthlyFee, FeeDueStatus.Unpaid, true);

            var pct = Math.Clamp(discountPercent, 0m, 100m);
            var discounted = Math.Round(monthlyFee * (100m - pct) / 100m, 2);

            // Fully waived (leave or 100% scholarship) → Waived, no late fee
            if (pct >= 100m || discounted <= 0m)
                return (0m, FeeDueStatus.Waived, false);

            // Partial scholarship → reduced base, no late fee on concession months
            return (discounted, FeeDueStatus.Unpaid, false);
        }

        /// <summary>
        /// Computes the label month for a due using the 25th-rule:
        /// if the due day is on or after the 25th, the payment is
        /// considered an advance for the NEXT calendar month.
        /// </summary>
        public static DateTime ComputeLabelMonth(DateTime collectionMonth, int dueDay) =>
            dueDay >= 25
                ? new DateTime(collectionMonth.Year, collectionMonth.Month, 1).AddMonths(1)
                : new DateTime(collectionMonth.Year, collectionMonth.Month, 1);
    }
}
