using System;
using System.Collections.Generic;
using FinTech.Domain.Common;
using FinTech.Domain.Events.FixedAssets;
using FinTech.Domain.Entities.Common;

namespace FinTech.Domain.Entities.FixedAssets
{
    public class FixedAsset : BaseEntity
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