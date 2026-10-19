namespace OffersService.Extensions;

public static class DateTimeExtensions
{
    /// <summary>
    /// Marks database date values as UTC when the provider returns unspecified DateTime instances.
    /// </summary>
    public static DateTime EnsureUtc(this DateTime dateTime)
    {
        return dateTime.Kind == DateTimeKind.Utc
            ? dateTime
            : DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
    }

    public static DateTime DateMax(this DateTime dateTime)
    {
        return dateTime.Kind == DateTimeKind.Utc
            ? dateTime
            : DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
    }
}
