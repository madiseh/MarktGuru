using System.ComponentModel.DataAnnotations;

namespace OffersService.Models;

public class Retailer
{
    public int Id { get; set; }

    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(2)]
    public string Country { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public ICollection<Offer> Offers { get; set; } = new List<Offer>();
}
