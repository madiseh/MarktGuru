namespace OffersService.DTOs;

public class OfferRow
{
    public int ProductId { get; set; }
    public decimal Price { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
}
