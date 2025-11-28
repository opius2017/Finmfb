using System;

namespace FinTech.Core.Domain.Entities.Common
{
    /// <summary>
    /// Base entity for all domain entities with common properties
    /// </summary>
    public abstract class BaseEntity
    {
        public string Id { get; protected set; } = Guid.NewGuid().ToString();
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public string? LastModifiedBy { get; set; }

        // Compatibility shims used by Application layer (transitional)
        public DateTime CreatedAt { get => CreatedDate; set => CreatedDate = value; }
        public DateTime? UpdatedAt { get => LastModifiedDate; set => LastModifiedDate = value; }

        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public Guid IdGuid
        {
            get
            {
                return Guid.TryParse(Id, out var g) ? g : Guid.Empty;
            }
            set
            {
                Id = value.ToString();
            }
        }
    }
}
