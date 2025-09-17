namespace FinTech.Application.DTOs.Loans
{
    public class RescheduleDto
    {
        public DateTime NewEndDate { get; set; }
        public string? Reason { get; set; }
        public string? ApprovedBy { get; set; }
    }
}