using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FinTech.Core.Application.DTOs.ClientPortal
{

    // Saved Payee DTOs
    public class SavedPayeeDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string AccountNumber { get; set; }
        public string BankName { get; set; }
        public string BankCode { get; set; }
        public string PayeeType { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastUsedDate { get; set; }
    }

    // Account Category DTOs
    public class AccountCategoryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Code { get; set; }
        public bool IsActive { get; set; }
    }
}