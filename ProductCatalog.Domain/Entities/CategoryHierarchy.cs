namespace ProductCatalog.Domain.Entities;

public partial class CategoryHierarchy : BaseEntity
{
    public Guid ParentId { get; set; }

    public Guid ChildId { get; set; }

    public virtual Category Child { get; set; } = null!;

    public virtual Category Parent { get; set; } = null!;
}
