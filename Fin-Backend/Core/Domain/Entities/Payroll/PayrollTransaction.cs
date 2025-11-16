using System;
using FinTech.Core.Domain.Common;
using FinTech.Core.Domain.Entities.Common;

namespace FinTech.Core.Domain.Entities.Payroll
{
    public class PayrollTransaction : BaseEntity
    {
        public string PayrollPeriodId { get; private set; }
        public string TransactionType { get; private set; }
        public decimal Amount { get; private set; }
        public string RelatedEntity { get; private set; }
        public string Reference { get; private set; }
        public string Description { get; private set; }
        public DateTime TransactionDate { get; private set; }

        private PayrollTransaction() { } // For EF Core

        public PayrollTransaction(
            string payrollPeriodId,
            string transactionType,
            decimal amount,
            string relatedEntity,
            string reference,
            string description)
        {
            PayrollPeriodId = payrollPeriodId;
            TransactionType = transactionType;
            Amount = amount;
            RelatedEntity = relatedEntity;
            Reference = reference;
            Description = description;
            TransactionDate = DateTime.UtcNow;
        }
    }
}
