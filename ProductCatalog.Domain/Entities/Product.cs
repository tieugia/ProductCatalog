using System.ComponentModel.DataAnnotations.Schema;

namespace ProductCatalog.Domain.Entities;

[Table("Products")]
public partial class Product : BaseEntity
{
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public int InventoryLevel { get; set; }

    public Guid CategoryId { get; set; }

    public string? ImageUrl { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Category Category { get; set; } = null!;
}
