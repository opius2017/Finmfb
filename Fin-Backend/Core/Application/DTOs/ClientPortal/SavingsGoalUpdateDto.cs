namespace FinTech.Core.Application.DTOs.ClientPortal
{
    public class SavingsGoalUpdateDto
    {
        public string Name { get; set; }
        public decimal TargetAmount { get; set; }
        public DateTime TargetDate { get; set; }
    }
}
