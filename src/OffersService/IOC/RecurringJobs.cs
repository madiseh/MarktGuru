using Hangfire;
using OffersService.Jobs;

namespace OffersService.IOC;

public static class RecurringJobs
{
    public static IApplicationBuilder UseApplicationRecurringJobs(this IApplicationBuilder app)
    {
        RecurringJob.AddOrUpdate<OfferExpiryJob>(
            OfferExpiryJob.JobId,
            job => job.ExecuteAsync(CancellationToken.None),
            Cron.Hourly());

        return app;
    }
}
