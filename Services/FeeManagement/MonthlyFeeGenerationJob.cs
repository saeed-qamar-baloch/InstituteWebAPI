using InstituteWebAPI.Services.FeeManagement;

namespace InstituteWebAPI.BackgroundJobs
{
    /// <summary>
    /// Hosted service that runs BulkGenerateMonthlyDuesAsync at the start of
    /// every calendar month (plus once on application startup to catch up on
    /// any missed months).
    ///
    /// Scheduling logic:
    ///   - On startup: runs immediately to catch up.
    ///   - Then waits until midnight on the 1st of the next month.
    ///   - After that: runs every 1st of the month at 00:05 UTC.
    /// </summary>
    public class MonthlyFeeGenerationJob : BackgroundService
    {
        private readonly IServiceScopeFactory scopeFactory;
        private readonly ILogger<MonthlyFeeGenerationJob> logger;

        public MonthlyFeeGenerationJob(
            IServiceScopeFactory scopeFactory,
            ILogger<MonthlyFeeGenerationJob> logger)
        {
            this.scopeFactory = scopeFactory;
            this.logger       = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Run once on startup (handles server restarts mid-month)
            await RunGenerationAsync(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                var delay = TimeUntilNextRun();
                logger.LogInformation(
                    "[MonthlyFeeJob] Next run scheduled in {h:0}h {m:0}m.",
                    delay.TotalHours, delay.Minutes);

                try { await Task.Delay(delay, stoppingToken); }
                catch (TaskCanceledException) { break; }

                await RunGenerationAsync(stoppingToken);
            }
        }

        private async Task RunGenerationAsync(CancellationToken ct)
        {
            logger.LogInformation("[MonthlyFeeJob] Starting monthly fee generation — {now:u}.", DateTime.UtcNow);
            try
            {
                using var scope   = scopeFactory.CreateScope();
                var svc           = scope.ServiceProvider.GetRequiredService<IFeeManagementService>();
                var result        = await svc.BulkGenerateMonthlyDuesAsync();

                logger.LogInformation(
                    "[MonthlyFeeJob] Done. Admissions processed: {p}, with new dues: {n}, total dues created: {t}, errors: {e}.",
                    result.AdmissionsProcessed,
                    result.AdmissionsWithNewDues,
                    result.TotalDuesCreated,
                    result.Errors.Count);

                if (result.Errors.Count > 0)
                {
                    foreach (var err in result.Errors)
                        logger.LogWarning("[MonthlyFeeJob] Skipped admission {id}: {reason}",
                            err.AdmissionId, err.Reason);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[MonthlyFeeJob] Unhandled error during generation.");
            }
        }

        /// <summary>
        /// Returns the time to wait until the 1st of next month at 00:05 UTC.
        /// If we are already past the 1st, waits until the 1st of the month after.
        /// Minimum delay of 30 seconds (prevents tight loops on the exact boundary).
        /// </summary>
        private static TimeSpan TimeUntilNextRun()
        {
            var now  = DateTime.UtcNow;
            var next = new DateTime(now.Year, now.Month, 1, 0, 5, 0, DateTimeKind.Utc)
                           .AddMonths(1);

            var delay = next - now;
            return delay < TimeSpan.FromSeconds(30) ? TimeSpan.FromSeconds(30) : delay;
        }
    }
}
