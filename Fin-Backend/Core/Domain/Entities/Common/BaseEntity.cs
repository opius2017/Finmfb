using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinTech.Core.Domain.Entities.Common
{
    /// <summary>
    /// Base entity for all domain entities with common properties
    /// </summary>
    public abstract class BaseEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public string? LastModifiedBy { get; set; }

        [NotMapped]
        public DateTime CreatedAt { get => CreatedDate; set => CreatedDate = value; }
        
        [NotMapped]
        public DateTime UpdatedAt { get => LastModifiedDate.GetValueOrDefault(); set => LastModifiedDate = value; }
        
        [NotMapped]
        public DateTime CreatedOn { get => CreatedDate; set => CreatedDate = value; }
        [NotMapped]
        public DateTime? LastModifiedOn { get => LastModifiedDate; set => LastModifiedDate = value; }
    }
}
