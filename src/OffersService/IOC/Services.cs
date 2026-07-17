using FluentValidation;
using OffersService.DTOs;
using OffersService.Services;
using OffersService.Validators;

namespace OffersService.IOC;

public static class Services
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IOfferService, OfferService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IOfferImportService, OfferImportService>();
        services.AddScoped<IRetailerService, RetailerService>();
        services.AddScoped<IValidator<CreateRetailerRequest>, CreateRetailerRequestValidator>();
        services.AddScoped<IValidator<OfferListRequest>, OfferListRequestValidator>();
        services.AddScoped<IValidator<RetailerStatsRequest>, RetailerStatsRequestValidator>();
        services.AddScoped<IValidator<OfferRow>, OfferRowValidator>();
        services.AddScoped<IValidator<ImportRequest>, ImportRequestValidator>();

        return services;
    }
}
