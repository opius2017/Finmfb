using FinTech.Domain.Common;
using FinTech.Domain.Entities.Common;

namespace FinTech.Domain.Entities.Customers
{
    public class CustomerDocument : BaseEntity
    {
        public int CustomerId { get; set; }
        public virtual Customer Customer { get; set; }
        public string DocumentType { get; set; }
        public string DocumentUrl { get; set; }
    }
}
