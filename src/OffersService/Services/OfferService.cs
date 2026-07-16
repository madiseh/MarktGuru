using Microsoft.EntityFrameworkCore;
using OffersService.Data;
using OffersService.DTOs;
using OffersService.Models;

namespace OffersService.Services;

public class OfferService : IOfferService
{
    private readonly AppDbContext _context;

    public OfferService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<OfferDto>> GetActiveOffersAsync()
    {
        var now = DateTime.UtcNow;
        var offers = await _context.Offers
            .Include(o => o.Product)
            .Where(o => o.ValidTo >= now && o.Status == OfferStatus.Active)
            .ToListAsync();

        return offers.Select(o => new OfferDto(o));
    }

    public async Task<OfferDto?> GetByIdAsync(int id)
    {
        var offer = await _context.Offers
            .Include(o => o.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        return offer is null ? null : new OfferDto(offer);
    }
}
