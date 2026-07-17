using Microsoft.EntityFrameworkCore;
using OffersService.Data;
using OffersService.DTOs;
using OffersService.Extensions;
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

    /// <summary>
    /// Returns offers that are active right now based on status and validity dates.
    /// </summary>
    public async Task<IEnumerable<OfferDto>> GetActiveOffersAsync()
    {
        var now = DateTime.UtcNow;
        var offers = await _context.Offers
            .Include(o => o.Product)
            .Where(o => o.ValidFrom <= now && o.ValidTo >= now && o.Status == OfferStatus.Active)
            .ToListAsync();

        return offers.Select(o => new OfferDto(o));
    }

    /// <summary>
    /// Returns a single offer by id, including product data, or null when it does not exist.
    /// </summary>
    public async Task<OfferDto?> GetByIdAsync(int id)
    {
        var offer = await _context.Offers
            .Include(o => o.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        return offer is null ? null : new OfferDto(offer);
    }

    /// <summary>
    /// Returns a filtered offer page and the unpaged total count using one EF Core query execution.
    /// </summary>
    public async Task<PagedList<OfferListItemDto>> GetOffersAsync(OfferListRequest request)
    {
        var now = DateTime.UtcNow;
        var filteredOffers = BuildFilteredOffersQuery(request, now);
        var offset = (request.Page - 1) * request.PageSize;

        // Build the requested page as an IQueryable; it is not executed yet.
        var pageQuery = filteredOffers
            .OrderBy(offer => offer.Id)
            .Skip(offset)
            .Take(request.PageSize)
            .Select(offer => new OfferListQueryRow
            {
                Id = offer.Id,
                ProductId = offer.ProductId,
                ProductName = offer.Product.Name,
                RetailerId = offer.RetailerId,
                RetailerName = offer.Retailer!.Name,
                Price = offer.Price,
                ValidFrom = offer.ValidFrom,
                ValidTo = offer.ValidTo,
                Status = offer.Status
            });

        // Execute one EF query that returns the unpaged total plus the requested page rows.
        var rows = await filteredOffers
            .GroupBy(_ => 1)
            .Select(group => new { TotalCount = group.Count() })
            .SelectMany(
                count => pageQuery.DefaultIfEmpty(),
                (count, item) => new OfferListQueryRow
                {
                    TotalCount = count.TotalCount,
                    Id = item == null ? null : item.Id,
                    ProductId = item == null ? null : item.ProductId,
                    ProductName = item == null ? null : item.ProductName,
                    RetailerId = item == null ? null : item.RetailerId,
                    RetailerName = item == null ? null : item.RetailerName,
                    Price = item == null ? null : item.Price,
                    ValidFrom = item == null ? null : item.ValidFrom,
                    ValidTo = item == null ? null : item.ValidTo,
                    Status = item == null ? null : item.Status
                })
            .ToListAsync();

        var totalCount = rows.FirstOrDefault()?.TotalCount ?? 0;

        // Map the in-memory query rows to the public DTO; this does not hit the database.
        var items = rows
            .Where(row => row.Id is not null)
            .Select(row => new OfferListItemDto(
                row.Id!.Value,
                row.ProductId!.Value,
                row.ProductName!,
                row.RetailerId!.Value,
                row.RetailerName!,
                row.Price!.Value,
                row.ValidFrom!.Value.EnsureUtc(),
                row.ValidTo!.Value.EnsureUtc(),
                GetEffectiveStatus(row.Status!.Value, row.ValidFrom.Value, row.ValidTo.Value, now)))
            .ToList();

        return PagedList<OfferListItemDto>.Create(items, request.Page, request.PageSize, totalCount);
    }

    /// <summary>
    /// Applies retailer-aware offer list filters without executing the query.
    /// </summary>
    private IQueryable<Offer> BuildFilteredOffersQuery(OfferListRequest request, DateTime now)
    {
        var filteredOffers = _context.Offers
            .AsNoTracking()
            .Where(offer => offer.RetailerId != null);

        if (request.ProductId is not null)
        {
            filteredOffers = filteredOffers.Where(offer => offer.ProductId == request.ProductId.Value);
        }

        if (request.RetailerId is not null)
        {
            filteredOffers = filteredOffers.Where(offer => offer.RetailerId == request.RetailerId.Value);
        }

        return request.Status switch
        {
            OfferListStatus.Active => filteredOffers.Where(offer =>
                offer.Status == OfferStatus.Active &&
                offer.ValidFrom <= now &&
                offer.ValidTo >= now),
            OfferListStatus.Expired => filteredOffers.Where(offer =>
                offer.Status == OfferStatus.Expired ||
                offer.ValidTo < now),
            _ => filteredOffers
        };
    }

    /// <summary>
    /// Converts persisted status and validity dates into the status exposed by the list endpoint.
    /// </summary>
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

    /// <summary>
    /// Represents a single-query offer list row: the unpaged total count plus an optional offer row.
    /// Offer fields are nullable so an empty page can still return the total count without a second query.
    /// </summary>
    private sealed class OfferListQueryRow
    {
        public int TotalCount { get; set; }
        public int? Id { get; set; }
        public int? ProductId { get; set; }
        public string? ProductName { get; set; }
        public int? RetailerId { get; set; }
        public string? RetailerName { get; set; }
        public decimal? Price { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public OfferStatus? Status { get; set; }
    }
}
