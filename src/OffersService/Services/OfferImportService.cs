using OffersService.Data;
using OffersService.DTOs;
using OffersService.Models;

namespace OffersService.Services;

public class OfferImportService : IOfferImportService
{
    private readonly AppDbContext _context;

    public OfferImportService(AppDbContext context)
    {
        _context = context;
    }

    public async void ImportAsync(IEnumerable<OfferRow> rows)
    {
        foreach (var row in rows)
        {
            await _context.Offers.AddAsync(new Offer
            {
                ProductId = row.ProductId,
                Price     = row.Price,
                ValidFrom = row.ValidFrom,
                ValidTo   = row.ValidTo,
                Status    = OfferStatus.Active,
                CreatedAt = DateTime.UtcNow
            });
        }
        await _context.SaveChangesAsync();
    }
}
