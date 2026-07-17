using System.Globalization;

namespace OffersService.DTOs;

public record RetailerStatsRequest
{
    public string Id { get; set; } = string.Empty;

    public int ToRetailerId()
    {
        return int.Parse(Id, CultureInfo.InvariantCulture);
    }
}
