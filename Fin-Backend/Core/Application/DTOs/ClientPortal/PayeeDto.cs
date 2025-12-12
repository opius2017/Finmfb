using System;

namespace FinTech.Core.Application.DTOs.ClientPortal
{
    public class SavedPayeeCreateDto
    {
        public string Name { get; set; }
        public string AccountNumber { get; set; }
        public string BankName { get; set; }
        public string BankCode { get; set; }
        public string CustomerReferenceNumber { get; set; }
    }

    public class AccountCategoryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Color { get; set; }
        public string Icon { get; set; }
    }
}
