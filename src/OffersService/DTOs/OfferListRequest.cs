namespace OffersService.DTOs;

public class OfferListRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int? ProductId { get; set; }
    public int? RetailerId { get; set; }
    public OfferListStatus Status { get; set; } = OfferListStatus.Active;
}
