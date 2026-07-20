namespace OffersService.DTOs;

public class OfferListRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int? ProductId { get; set; }
    public int? RetailerId { get; set; }
    public string Status { get; set; } = "active";

    public OfferListStatus ToStatus()
    {
        return Status.Trim().ToLowerInvariant() switch
        {
            "active" => OfferListStatus.Active,
            "expired" => OfferListStatus.Expired,
            "all" => OfferListStatus.All,
            _ => throw new InvalidOperationException("Offer list status must be validated before use.")
        };
    }
}
