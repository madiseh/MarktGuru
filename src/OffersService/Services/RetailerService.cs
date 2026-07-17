using Microsoft.EntityFrameworkCore;
using OffersService.Data;
using OffersService.DTOs;
using OffersService.Models;

namespace OffersService.Services;

public class RetailerService : IRetailerService
{
    private readonly AppDbContext _context;

    public RetailerService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<RetailerDto?> GetByIdAsync(int id)
    {
        return await _context.Retailers
            .AsNoTracking()
            .Where(retailer => retailer.Id == id)
            .Select(retailer => new RetailerDto(
                retailer.Id,
                retailer.Name,
                retailer.Country,
                retailer.IsActive,
                EnsureUtc(retailer.CreatedAt)))
            .FirstOrDefaultAsync();
    }

    public async Task<RetailerDto?> CreateAsync(CreateRetailerRequest request)
    {
        var name = request.Name!.Trim();
        var country = request.Country!.Trim().ToUpperInvariant();
        var nameKey = name.ToUpperInvariant();

        var retailerExists = await _context.Retailers
            .AnyAsync(retailer => retailer.Name.ToUpper() == nameKey);

        if (retailerExists)
        {
            return null;
        }

        var retailer = new Retailer
        {
            Name = name,
            Country = country,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Retailers.Add(retailer);
        await _context.SaveChangesAsync();

        return new RetailerDto(
            retailer.Id,
            retailer.Name,
            retailer.Country,
            retailer.IsActive,
            EnsureUtc(retailer.CreatedAt));
    }

    public async Task<RetailerStatsDto?> GetStatsAsync(int id)
    {
        var now = DateTime.UtcNow;

        var stats = await _context.Retailers
            .AsNoTracking()
            .Where(retailer => retailer.Id == id && retailer.IsActive)
            .Select(retailer => new
            {
                retailer.Id,
                retailer.Name,
                retailer.Country,
                retailer.IsActive,
                ActiveOfferCount = retailer.Offers.Count(offer =>
                    offer.Status == OfferStatus.Active &&
                    offer.ValidFrom <= now &&
                    offer.ValidTo >= now),
                AvgPrice = retailer.Offers
                    .Where(offer =>
                        offer.Status == OfferStatus.Active &&
                        offer.ValidFrom <= now &&
                        offer.ValidTo >= now)
                    .Average(offer => (double?)offer.Price),
                LowestPrice = retailer.Offers
                    .Where(offer =>
                        offer.Status == OfferStatus.Active &&
                        offer.ValidFrom <= now &&
                        offer.ValidTo >= now)
                    .Min(offer => (double?)offer.Price),
                MostRecentActiveOffer = retailer.Offers
                    .Where(offer =>
                        offer.Status == OfferStatus.Active &&
                        offer.ValidFrom <= now &&
                        offer.ValidTo >= now)
                    .OrderByDescending(offer => offer.ValidFrom)
                    .ThenByDescending(offer => offer.Id)
                    .Select(offer => new
                    {
                        offer.Id,
                        ProductName = offer.Product.Name,
                        Price = (double)offer.Price,
                        offer.ValidTo
                    })
                    .FirstOrDefault()
            })
            .FirstOrDefaultAsync();

        if (stats is null)
        {
            return null;
        }

        return new RetailerStatsDto(
            stats.Id,
            stats.Name,
            stats.Country,
            stats.IsActive,
            stats.ActiveOfferCount,
            stats.AvgPrice is null ? null : Convert.ToDecimal(stats.AvgPrice.Value),
            stats.LowestPrice is null ? null : Convert.ToDecimal(stats.LowestPrice.Value),
            stats.MostRecentActiveOffer is null
                ? null
                : new MostRecentActiveOfferDto(
                    stats.MostRecentActiveOffer.Id,
                    stats.MostRecentActiveOffer.ProductName,
                    Convert.ToDecimal(stats.MostRecentActiveOffer.Price),
                    EnsureUtc(stats.MostRecentActiveOffer.ValidTo)));
    }

    private static DateTime EnsureUtc(DateTime dateTime)
    {
        return dateTime.Kind == DateTimeKind.Utc
            ? dateTime
            : DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
    }
}
