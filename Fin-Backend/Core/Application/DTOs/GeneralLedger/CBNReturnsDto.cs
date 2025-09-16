using System;
using System.Collections.Generic;

namespace FinTech.Core.Application.DTOs.GeneralLedger
{
    public class CBNReturnsDto
    {
        public string ReturnType { get; set; }
        public DateTime AsOfDate { get; set; }
        public DateTime GeneratedDate { get; set; }
        public IDictionary<string, decimal> Values { get; set; }
        public IEnumerable<CBNReturnItemDto> ReturnItems { get; set; }
    }
}