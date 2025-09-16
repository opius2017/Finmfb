using System;
using System.Collections.Generic;

namespace FinTech.Core.Application.DTOs.GeneralLedger.Regulatory
{
    public class IFRSDisclosureDto
    {
        public string DisclosureType { get; set; }
        public DateTime AsOfDate { get; set; }
        public DateTime GeneratedDate { get; set; }
        public IDictionary<string, object> DisclosureData { get; set; }
    }
}