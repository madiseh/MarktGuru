using OffersService.Jobs;

namespace OffersService.IOC;

public static class Jobs
{
    public static IServiceCollection AddApplicationJobs(this IServiceCollection services)
    {
        services.AddScoped<OfferExpiryJob>();

        return services;
    }
}
