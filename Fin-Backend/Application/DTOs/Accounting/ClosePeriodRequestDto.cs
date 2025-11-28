namespace FinTech.Core.Application.DTOs.Accounting
{
    public class ClosePeriodRequestDto
    {
        public string PeriodId { get; set; }
        public bool ForceClose { get; set; }
    }
}
