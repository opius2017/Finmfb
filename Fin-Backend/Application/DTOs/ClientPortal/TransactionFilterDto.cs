using System.Collections.Generic;

namespace FinTech.Core.Application.DTOs.ClientPortal
{
    public class TransactionFilterDto
    {
        public string AccountNumber { get; set; }
        public List<string> TransactionTypes { get; set; } = new List<string>();
        public List<string> Categories { get; set; } = new List<string>();
        public List<string> Channels { get; set; } = new List<string>();
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
    }
}
