using System.ComponentModel.DataAnnotations;

namespace ProductCatalog.Domain.Entities
{
    public abstract class BaseEntity
    {
        public Guid Id { get; set; }
        [Timestamp]
        public byte[] RowVersion { get; set; } = null!;
    }
}
