using System;
using System.Collections.Generic;
using FinTech.Core.Domain.Common;
using FinTech.Core.Domain.Events.FixedAssets;
using FinTech.Core.Domain.Entities.Common;

namespace FinTech.Core.Domain.Entities.FixedAssets
{
    public class FixedAsset : AggregateRoot
    {
        public string AssetCode { get; private set; }
        public string AssetName { get; private set; }
        public string Description { get; private set; }
        public string AssetCategory { get; private set; }
        public DateTime AcquisitionDate { get; private set; }
        public decimal AcquisitionCost { get; private set; }
        public decimal AccumulatedDepreciation { get; private set; }
        public decimal CurrentValue { get; private set; }
        public decimal ResidualValue { get; private set; }
        public decimal UsefulLifeYears { get; private set; }
        public string DepreciationMethod { get; private set; }
        public string Location { get; private set; }
        public string AssetStatus { get; private set; }
        public DateTime? DisposalDate { get; private set; }
        public string SerialNumber { get; private set; }
        public string Model { get; private set; }
        public string Manufacturer { get; private set; }
        public virtual ICollection<AssetTransaction> Transactions { get; private set; } = new List<AssetTransaction>();

            // Compatibility / shim properties used by Application layer (transitional)
            // AssetNumber maps to AssetCode
            public string AssetNumber
            {
                get => AssetCode;
                set => AssetCode = value;
            }

            // PurchaseCost maps to AcquisitionCost
            public decimal PurchaseCost
            {
                get => AcquisitionCost;
                set => AcquisitionCost = value;
            }

            // DepreciableAmount computed from acquisition cost minus residual value
            public decimal DepreciableAmount
            {
                get => AcquisitionCost - ResidualValue;
                set => AcquisitionCost = value + ResidualValue;
            }

            // CurrentBookValue maps to CurrentValue
            public decimal CurrentBookValue
            {
                get => CurrentValue;
                set => CurrentValue = value;
            }

            // PurchaseDate maps to AcquisitionDate
            public DateTime PurchaseDate
            {
                get => AcquisitionDate;
                set => AcquisitionDate = value;
            }

            // InServiceDate - separate field (may be same as AcquisitionDate)
            private DateTime? _inServiceDate;
            public DateTime? InServiceDate
            {
                get => _inServiceDate ?? AcquisitionDate;
                set => _inServiceDate = value;
            }

            // Accounting integration account ids (no-op for domain but present in Application DTOs)
            public string AssetAccountId { get; set; }
            public string AccumulatedDepreciationAccountId { get; set; }
            public string DepreciationExpenseAccountId { get; set; }
            public string DisposalGainLossAccountId { get; set; }

            // Last depreciation tracking
            public DateTime? LastDepreciationDate { get; set; }

            // Provide enum-backed Status compatible with Application FixedAssetStatus
            public FinTech.Core.Domain.Enums.FixedAssetStatus Status
            {
                get
                {
                    if (string.IsNullOrWhiteSpace(AssetStatus)) return FinTech.Core.Domain.Enums.FixedAssetStatus.Active;
                    if (Enum.TryParse<FinTech.Core.Domain.Enums.FixedAssetStatus>(AssetStatus, true, out var s))
                        return s;
                    return FinTech.Core.Domain.Enums.FixedAssetStatus.Active;
                }
                set
                {
                    AssetStatus = value.ToString().ToUpper();
                }
            }

            // Other common fields used by Application
            public string AssetTag { get; set; } = string.Empty;
            public DateTime? WarrantyExpiryDate { get; set; }
            public string Notes { get; set; } = string.Empty;

            // Disposal-related compatibility properties
            public string DisposalType { get; set; }
            public decimal? DisposalAmount { get; set; }
            public decimal? DisposalGainLoss { get; set; }

        private FixedAsset() { } // For EF Core

        public FixedAsset(
            string assetCode,
            string assetName,
            string description,
            string assetCategory,
            DateTime acquisitionDate,
            decimal acquisitionCost,
            decimal residualValue,
            decimal usefulLifeYears,
            string depreciationMethod,
            string location,
            string serialNumber,
            string model,
            string manufacturer)
        {
            AssetCode = assetCode;
            AssetName = assetName;
            Description = description;
            AssetCategory = assetCategory;
            AcquisitionDate = acquisitionDate;
            AcquisitionCost = acquisitionCost;
            AccumulatedDepreciation = 0;
            CurrentValue = acquisitionCost;
            ResidualValue = residualValue;
            UsefulLifeYears = usefulLifeYears;
            DepreciationMethod = depreciationMethod;
            Location = location;
            AssetStatus = "ACTIVE";
            SerialNumber = serialNumber;
            Model = model;
            Manufacturer = manufacturer;
        }

        public void RecordAcquisition(decimal taxAmount, string reference, string description)
        {
            var transaction = new AssetTransaction(
                this.Id,
                "ACQUISITION",
                AcquisitionCost,
                reference,
                description);
            
            Transactions.Add(transaction);

            // Raise domain event
            AddDomainEvent(new AssetAcquiredEvent(
                this.Id,
                AcquisitionCost,
                taxAmount,
                AssetCategory,
                reference,
                description));
        }

        public void RecordDepreciation(decimal depreciationAmount, string period, string reference, string description)
        {
            if (AssetStatus != "ACTIVE")
                throw new InvalidOperationException("Cannot depreciate an inactive asset");

            if (depreciationAmount <= 0)
                throw new ArgumentException("Depreciation amount must be positive", nameof(depreciationAmount));

            if (CurrentValue - depreciationAmount < ResidualValue)
                depreciationAmount = CurrentValue - ResidualValue;

            AccumulatedDepreciation += depreciationAmount;
            CurrentValue -= depreciationAmount;

            var transaction = new AssetTransaction(
                this.Id,
                "DEPRECIATION",
                depreciationAmount,
                reference,
                description);
            
            Transactions.Add(transaction);

            // Raise domain event
            AddDomainEvent(new AssetDepreciatedEvent(
                this.Id,
                depreciationAmount,
                period,
                reference,
                description));
        }

        public void Dispose(decimal disposalProceeds, string reference, string description)
        {
            if (AssetStatus != "ACTIVE")
                throw new InvalidOperationException("Cannot dispose an inactive asset");

            decimal netBookValue = CurrentValue;
            decimal gainLoss = disposalProceeds - netBookValue;

            AssetStatus = "DISPOSED";
            DisposalDate = DateTime.UtcNow;

            var transaction = new AssetTransaction(
                this.Id,
                "DISPOSAL",
                disposalProceeds,
                reference,
                description);
            
            Transactions.Add(transaction);

            // Raise domain event
            AddDomainEvent(new AssetDisposedEvent(
                this.Id,
                disposalProceeds,
                netBookValue,
                gainLoss,
                reference,
                description));
        }

        public void Revalue(decimal revaluationAmount, string reference, string description)
        {
            if (AssetStatus != "ACTIVE")
                throw new InvalidOperationException("Cannot revalue an inactive asset");

            decimal oldValue = CurrentValue;
            CurrentValue += revaluationAmount;

            if (CurrentValue < 0)
                throw new ArgumentException("Revaluation cannot result in a negative asset value");

            var transaction = new AssetTransaction(
                this.Id,
                "REVALUATION",
                revaluationAmount,
                reference,
                description);
            
            Transactions.Add(transaction);

            // Raise domain event
            AddDomainEvent(new AssetRevaluedEvent(
                this.Id,
                revaluationAmount,
                reference,
                description));
        }

        public void RecordImpairment(decimal impairmentAmount, string reference, string description)
        {
            if (AssetStatus != "ACTIVE")
                throw new InvalidOperationException("Cannot impair an inactive asset");

            if (impairmentAmount <= 0)
                throw new ArgumentException("Impairment amount must be positive", nameof(impairmentAmount));

            if (CurrentValue - impairmentAmount < 0)
                impairmentAmount = CurrentValue;

            CurrentValue -= impairmentAmount;

            var transaction = new AssetTransaction(
                this.Id,
                "IMPAIRMENT",
                impairmentAmount,
                reference,
                description);
            
            Transactions.Add(transaction);

            // Raise domain event
            AddDomainEvent(new AssetImpairedEvent(
                this.Id,
                impairmentAmount,
                reference,
                description));
        }

        public void Transfer(string newLocation, string reference, string description)
        {
            if (AssetStatus != "ACTIVE")
                throw new InvalidOperationException("Cannot transfer an inactive asset");

            string oldLocation = Location;
            Location = newLocation;

            var transaction = new AssetTransaction(
                this.Id,
                "TRANSFER",
                0,
                reference,
                description);
            
            Transactions.Add(transaction);
        }
    }
}
