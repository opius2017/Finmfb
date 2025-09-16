using System;
using FinTech.Domain.Common;

namespace FinTech.Domain.Entities.FixedAssets
{
    public class AssetTransaction : BaseEntity
    {
        public int FixedAssetId { get; private set; }
        public string TransactionType { get; private set; }
        public decimal Amount { get; private set; }
        public string Reference { get; private set; }
        public string Description { get; private set; }
        public DateTime TransactionDate { get; private set; }

        private AssetTransaction() { } // For EF Core

        public AssetTransaction(
            int fixedAssetId,
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