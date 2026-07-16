using System.ComponentModel.DataAnnotations;

namespace OffersService.Models;

public class Product
{
    public int Id { get; set; }

    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Category { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public ICollection<Offer> Offers { get; set; } = new List<Offer>();
}
