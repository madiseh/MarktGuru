namespace OffersService.DTOs;

public class ImportRequest
{
    public IEnumerable<OfferRow> Rows { get; set; } = Enumerable.Empty<OfferRow>();
}
