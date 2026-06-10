using InstituteWebApp.Models.Domain;

namespace InstituteWebAPI.Helpers
{
    /// <summary>
    /// Resolves a grade label from a percentage using either a configured
    /// list of <see cref="GradeCriteria"/> or built-in defaults.
    ///
    /// This is the single authoritative place for grade calculation.
    /// Both <see cref="Controllers.StudentResultCardController"/> and any
    /// future reporting code must call this instead of embedding their own
    /// switch expressions.
    /// </summary>
    public static class GradeCalculator
    {
        /// <summary>
        /// Default grade thresholds used when no <see cref="GradeCriteria"/>
        /// are configured in the database.  Ordered highest-first.
        /// </summary>
        private static readonly (float MinPct, string Label)[] DefaultGrades =
        {
            (80f, "A+"),
            (70f, "A"),
            (60f, "B"),
            (50f, "C"),
            (45f, "D"),
            (33f, "E"),
            (0f,  "F"),
        };

        /// <summary>
        /// Returns a grade label for <paramref name="percentage"/> using the
        /// supplied <paramref name="criteria"/> list.
        ///
        /// Criteria are matched by finding the highest <see cref="GradeCriteria.MinPercentage"/>
        /// that is ≤ <paramref name="percentage"/> (i.e. the student meets that threshold).
        /// Falls back to built-in defaults when <paramref name="criteria"/> is null or empty.
        /// </summary>
        public static string Resolve(float percentage, IEnumerable<GradeCriteria>? criteria)
        {
            if (criteria != null)
            {
                // Sort descending so the first match is the highest qualifying grade.
                var sorted = criteria
                    .OrderByDescending(c => c.MinPercentage)
                    .ToList();

                if (sorted.Count > 0)
                {
                    foreach (var c in sorted)
                    {
                        if (percentage >= c.MinPercentage)
                            return c.GradeLabel;
                    }
                    // Below the lowest threshold → return the last (lowest) grade label
                    return sorted[^1].GradeLabel;
                }
            }

            // Fallback to built-in defaults
            return ResolveFromDefaults(percentage);
        }

        /// <summary>
        /// Resolves a grade using only the built-in default thresholds.
        /// Useful in contexts where loading DB criteria would be impractical
        /// (e.g. bulk report generation already running on a tight loop).
        /// </summary>
        public static string ResolveFromDefaults(float percentage)
        {
            foreach (var (minPct, label) in DefaultGrades)
            {
                if (percentage >= minPct)
                    return label;
            }
            return "F";
        }
    }
}
