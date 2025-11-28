using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FinTech.Core.Application.DTOs.ClientPortal
{

    public class SavedPayeeUpdateDto
    {
        public string Name { get; set; }
        public string AccountNumber { get; set; }
        public string BankName { get; set; }
        public string BankCode { get; set; }
        public string CustomerReferenceNumber { get; set; }
    }

    // Alias for historic name; canonical create DTO is CreateSavedPayeeDto in ClientPortalDTOs.cs
    public class SavedPayeeCreateDto : CreateSavedPayeeDto { }
}