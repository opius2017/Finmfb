using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FinTech.Core.Application.DTOs.ClientPortal
{
    public class DocumentTypeDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsRequired { get; set; }
        public string Category { get; set; }
        public List<string> AcceptedFormats { get; set; }
        public long MaxFileSize { get; set; }
    }
}