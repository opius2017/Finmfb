using System;
using FinTech.Core.Domain.Common;
using FinTech.Core.Domain.Entities.FixedAssets;

namespace FinTech.Core.Domain.Events.FixedAssets
{
    public class AssetAcquiredEvent : DomainEvent
    {
        public string AssetId { get; }
        public decimal AcquisitionCost { get; }
        public decimal TaxAmount { get; }
        public string AssetCategory { get; }
        public string Reference { get; }
        public string Description { get; }

        public AssetAcquiredEvent(string assetId, decimal acquisitionCost, decimal taxAmount, 
            string assetCategory, string reference, string description)
        {
            AssetId = assetId;
            AcquisitionCost = acquisitionCost;
            TaxAmount = taxAmount;
            AssetCategory = assetCategory;
            Reference = reference;
            Description = description;
        }
    }

    public class AssetDepreciatedEvent : DomainEvent
    {
        public string AssetId { get; }
        public decimal DepreciationAmount { get; }
        public string Period { get; }
        public string Reference { get; }
        public string Description { get; }

        public AssetDepreciatedEvent(string assetId, decimal depreciationAmount, string period, string reference, string description)
        {
            AssetId = assetId;
            DepreciationAmount = depreciationAmount;
            Period = period;
            Reference = reference;
            Description = description;
        }
    }

    public class AssetDisposedEvent : DomainEvent
    {
        public string AssetId { get; }
        public decimal DisposalProceeds { get; }
        public decimal NetBookValue { get; }
        public decimal GainLoss { get; }
        public string Reference { get; }
        public string Description { get; }

        public AssetDisposedEvent(string assetId, decimal disposalProceeds, decimal netBookValue, 
            decimal gainLoss, string reference, string description)
        {
            AssetId = assetId;
            DisposalProceeds = disposalProceeds;
            NetBookValue = netBookValue;
            GainLoss = gainLoss;
            Reference = reference;
            Description = description;
        }
    }

    public class AssetRevaluedEvent : DomainEvent
    {
        public string AssetId { get; }
        public decimal RevaluationAmount { get; }
        public string Reference { get; }
        public string Description { get; }

        public AssetRevaluedEvent(string assetId, decimal revaluationAmount, string reference, string description)
        {
            AssetId = assetId;
            RevaluationAmount = revaluationAmount;
            Reference = reference;
            Description = description;
        }
    }

    public class AssetImpairedEvent : DomainEvent
    {
        public string AssetId { get; }
        public decimal ImpairmentAmount { get; }
        public string Reference { get; }
        public string Description { get; }

        public AssetImpairedEvent(string assetId, decimal impairmentAmount, string reference, string description)
        {
            AssetId = assetId;
            ImpairmentAmount = impairmentAmount;
            Reference = reference;
            Description = description;
        }
    }
}
