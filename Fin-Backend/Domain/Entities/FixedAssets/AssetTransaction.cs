using System;
using FinTech.Core.Domain.Common;
using FinTech.Core.Domain.Entities.Common;

namespace FinTech.Core.Domain.Entities.FixedAssets
{
    public class AssetTransaction : BaseEntity
    {
        public string FixedAssetId { get; private set; }
        public string TransactionType { get; private set; }
        public decimal Amount { get; private set; }
        public string Reference { get; private set; }
        public string Description { get; private set; }
        public DateTime TransactionDate { get; private set; }

        private AssetTransaction() { } // For EF Core

        public AssetTransaction(
            string fixedAssetId,
            string transactionType,
            decimal amount,
            string reference,
            string description)
        {
            FixedAssetId = fixedAssetId;
            TransactionType = transactionType;
            Amount = amount;
            Reference = reference;
            Description = description;
            TransactionDate = DateTime.UtcNow;
        }
    }
}
