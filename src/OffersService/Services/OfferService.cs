using Microsoft.EntityFrameworkCore;
using OffersService.Data;
using OffersService.DTOs;
using OffersService.Models;
using OffersService.Models.Pagination;

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
            .Where(o => o.ValidFrom <= now && o.ValidTo >= now && o.Status == OfferStatus.Active)
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

    public async Task<PagedList<OfferListItemDto>> GetOffersAsync(OfferListRequest request)
    {
        var now = DateTime.UtcNow;

        var filteredOffers = _context.Offers
            .AsNoTracking()
            .Where(o => o.RetailerId != null);

        if (request.ProductId is not null)
        {
            filteredOffers = filteredOffers.Where(o => o.ProductId == request.ProductId.Value);
        }

        if (request.RetailerId is not null)
        {
            filteredOffers = filteredOffers.Where(o => o.RetailerId == request.RetailerId.Value);
        }

        filteredOffers = request.Status switch
        {
            OfferListStatus.Active => filteredOffers.Where(o =>
                o.Status == OfferStatus.Active &&
                o.ValidFrom <= now &&
                o.ValidTo >= now),
            OfferListStatus.Expired => filteredOffers.Where(o =>
                o.Status == OfferStatus.Expired ||
                o.ValidTo < now),
            _ => filteredOffers
        };

        var query = filteredOffers
            .OrderBy(o => o.Id)
            .Select(o => new OfferListItemProjection(
                o.Id,
                o.ProductId,
                o.Product.Name,
                o.RetailerId!.Value,
                o.Retailer!.Name,
                o.Price,
                o.ValidFrom,
                o.ValidTo,
                o.Status));

        var page = await PagedList<OfferListItemProjection>.CreateAsync(query, request.Page, request.PageSize);

        return page.Map(row => new OfferListItemDto(
            row.Id,
            row.ProductId,
            row.ProductName,
            row.RetailerId,
            row.RetailerName,
            row.Price,
            EnsureUtc(row.ValidFrom),
            EnsureUtc(row.ValidTo),
            GetEffectiveStatus(row.Status, row.ValidFrom, row.ValidTo, now)));
    }

    private static string GetEffectiveStatus(OfferStatus status, DateTime validFrom, DateTime validTo, DateTime now)
    {
        if (status == OfferStatus.Expired || validTo < now)
        {
            return OfferStatus.Expired.ToString();
        }

        if (status == OfferStatus.Active && validFrom <= now && validTo >= now)
        {
            return OfferStatus.Active.ToString();
        }

        return status.ToString();
    }

    private static DateTime EnsureUtc(DateTime dateTime)
    {
        return dateTime.Kind == DateTimeKind.Utc
            ? dateTime
            : DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
    }

    private record OfferListItemProjection(
        int Id,
        int ProductId,
        string ProductName,
        int RetailerId,
        string RetailerName,
        decimal Price,
        DateTime ValidFrom,
        DateTime ValidTo,
        OfferStatus Status);
}
