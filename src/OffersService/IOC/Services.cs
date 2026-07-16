using OffersService.Services;

namespace OffersService.IOC;

public static class Services
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IOfferService, OfferService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IOfferImportService, OfferImportService>();

        return services;
    }
}
