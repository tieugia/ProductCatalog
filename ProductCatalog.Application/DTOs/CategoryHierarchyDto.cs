using ProductCatalog.Common.Attributes;

namespace ProductCatalog.Application.DTOs
{
    public class CategoryHierarchyDto
    {
        public Guid Id { get; set; }
        [NotEmptyGuid]
        public Guid ParentId { get; set; }
        [NotEmptyGuid]
        public Guid ChildId { get; set; }
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}
