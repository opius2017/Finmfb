using FinTech.Core.Domain.Common;
using FinTech.Core.Domain.Entities.Common;
using FinTech.Core.Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinTech.Core.Domain.Entities.ClientPortal
{
    /// <summary>
    /// Represents a savings goal set by the client
    /// </summary>
    public class SavingsGoal : BaseEntity, IAuditable
    {
        public string ClientPortalProfileId { get; set; } = string.Empty;
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        [Required]
        public string AccountId { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(200)]
        public string GoalName { get; set; } = string.Empty;
        
        [MaxLength(1000)]
        public string? Description { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TargetAmount { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal CurrentAmount { get; set; }
        
        [Required]
        public DateTime StartDate { get; set; }
        
        [Required]
        public DateTime TargetDate { get; set; }
        
        [MaxLength(50)]
        public string GoalType { get; set; } = "General"; // General, Vacation, Emergency, Home, Car, Education, etc.
        
        [MaxLength(50)]
        public string Priority { get; set; } = "Medium"; // High, Medium, Low
        
        public bool IsActive { get; set; } = true;
        
        public bool AutomaticContribution { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal? MonthlyContribution { get; set; }
        
        [MaxLength(50)]
        public string? ContributionFrequency { get; set; } // Weekly, Bi-weekly, Monthly, Quarterly
        
        public DateTime? NextContributionDate { get; set; }
        
        [Column(TypeName = "decimal(5,2)")]
        public decimal? CompletionPercentage { get; set; }
        
        public DateTime? CompletedDate { get; set; }
        
        [MaxLength(50)]
        public string Status { get; set; } = "Active"; // Active, Completed, Paused, Cancelled
        
        [MaxLength(500)]
        public string? Notes { get; set; }
        
        [MaxLength(255)]
        public string? ImageUrl { get; set; } // Goal motivation image
        
        public bool IsPublic { get; set; } // For sharing progress with family
        
        public bool NotificationsEnabled { get; set; } = true;
        
        // Calculated properties
        [NotMapped]
        public decimal AmountRemaining => TargetAmount - CurrentAmount;
        
        [NotMapped]
        public int DaysRemaining => (TargetDate - DateTime.Now).Days;
        
        [NotMapped]
        public decimal RequiredMonthlyAmount => 
            DaysRemaining > 0 ? AmountRemaining / (DaysRemaining / 30.44m) : 0;
            
        public virtual ICollection<SavingsGoalTransaction> Transactions { get; set; } = new List<SavingsGoalTransaction>();
        public virtual ICollection<SavingsGoalMilestone> Milestones { get; set; } = new List<SavingsGoalMilestone>();
    }

    /// <summary>
    /// Represents transactions related to savings goals
    /// </summary>
    public class SavingsGoalTransaction : BaseEntity, IAuditable
    {
        [Required]
        public string SavingsGoalId { get; set; } = string.Empty;
        public SavingsGoal? SavingsGoal { get; set; }
        
        [Required]
        public string TransactionId { get; set; } = string.Empty;
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string TransactionType { get; set; } = string.Empty; // Contribution, Withdrawal, Interest, Bonus
        
        [Required]
        public DateTime TransactionDate { get; set; }
        
        [MaxLength(500)]
        public string? Description { get; set; }
        
        [MaxLength(100)]
        public string? Source { get; set; } // Manual, Automatic, Transfer, etc.
        
        public bool IsAutomated { get; set; }
        
        [MaxLength(50)]
        public string? PaymentMethod { get; set; } // Bank Transfer, Cash Deposit, etc.
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal BalanceAfter { get; set; }
        
        [MaxLength(255)]
        public string? Reference { get; set; }
        
        [MaxLength(50)]
        public string Status { get; set; } = "Completed"; // Pending, Completed, Failed, Reversed
        
        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Represents milestones achieved for savings goals
    /// </summary>
    public class SavingsGoalMilestone : BaseEntity, IAuditable
    {
        [Required]
        public string SavingsGoalId { get; set; } = string.Empty;
        public SavingsGoal? SavingsGoal { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string MilestoneName { get; set; } = string.Empty;
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TargetAmount { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(5,2)")]
        public decimal TargetPercentage { get; set; }
        
        public DateTime? AchievedDate { get; set; }
        
        public bool IsAchieved { get; set; }
        
        [MaxLength(500)]
        public string? Description { get; set; }
        
        [MaxLength(500)]
        public string? RewardDescription { get; set; }
        
        public bool NotificationSent { get; set; }
        
        [MaxLength(255)]
        public string? BadgeUrl { get; set; }
    }
}