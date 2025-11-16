using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FinTech.Core.Application.DTOs.ClientPortal
{

    public class BillerFieldDto
    {
        public string Name { get; set; }
        public string Label { get; set; }
        public string Type { get; set; }
        public bool IsRequired { get; set; }
        public string ValidationPattern { get; set; }
    }
}