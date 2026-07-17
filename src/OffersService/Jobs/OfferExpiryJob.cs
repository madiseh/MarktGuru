using Microsoft.EntityFrameworkCore;
using OffersService.Data;
using OffersService.Models;

namespace OffersService.Jobs;

public class OfferExpiryJob
{
    public const string JobId = "OfferExpiryJob";

    private readonly AppDbContext _context;
    private readonly ILogger<OfferExpiryJob> _logger;

    public OfferExpiryJob(AppDbContext context, ILogger<OfferExpiryJob> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<int> ExecuteAsync(CancellationToken cancellationToken)
    {
        var timestamp = DateTime.UtcNow;

        var expiredCount = await _context.Offers
            .Where(offer => offer.ValidTo < timestamp && offer.Status != OfferStatus.Expired)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(offer => offer.Status, OfferStatus.Expired),
                cancellationToken);

        _logger.LogInformation("Expired {Count} offers at {Timestamp}", expiredCount, timestamp);

        return expiredCount;
    }
}
