using System.ComponentModel.DataAnnotations.Schema;

namespace ProductCatalog.Domain.Entities;

[Table("Categories")]
public partial class Category : BaseEntity
{
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<CategoryHierarchy> Children { get; set; } = new List<CategoryHierarchy>();

    public virtual ICollection<CategoryHierarchy> Parents { get; set; } = new List<CategoryHierarchy>();

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
