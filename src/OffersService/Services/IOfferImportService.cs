using OffersService.DTOs;

namespace OffersService.Services;

public interface IOfferImportService
{
    void ImportAsync(IEnumerable<OfferRow> rows);
}
