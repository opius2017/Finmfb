namespace FinTech.Core.Application.DTOs.GeneralLedger.Regulatory
{
    public class NDICReturnItemDto
    {
        public string ItemCode { get; set; }
        public string ItemDescription { get; set; }
        public decimal Amount { get; set; }
        public string Category { get; set; }
    }
}
