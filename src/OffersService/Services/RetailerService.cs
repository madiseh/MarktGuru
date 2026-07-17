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
