using OffersService.Exceptions;

namespace OffersService.IOC;

public static class ExceptionHandling
{
    public static IServiceCollection AddApplicationExceptionHandling(this IServiceCollection services)
    {
        services.AddExceptionHandler<ValidationExceptionHandler>();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        return services;
    }

    public static IApplicationBuilder UseApplicationExceptionHandling(this IApplicationBuilder app)
    {
        app.UseExceptionHandler();

        return app;
    }
}
