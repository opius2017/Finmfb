using System;
using System.Collections.Generic;

namespace FinTech.Core.Application.DTOs.Accounting
{
    public class CreateFixedAssetDto
    {
        public string AssetNumber { get; set; } = string.Empty;
        public string AssetName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string AssetCategory { get; set; } = string.Empty;
        public string AssetAccountId { get; set; } = string.Empty;
        public string AccumulatedDepreciationAccountId { get; set; } = string.Empty;
        public string DepreciationExpenseAccountId { get; set; } = string.Empty;
        public string DisposalGainLossAccountId { get; set; } = string.Empty;
        public DateTime PurchaseDate { get; set; }
        public DateTime InServiceDate { get; set; }
        public decimal PurchaseCost { get; set; }
        public decimal ResidualValue { get; set; }
        public int UsefulLifeYears { get; set; }
        public DepreciationMethod DepreciationMethod { get; set; }
        public string Location { get; set; } = string.Empty;
        public string AssetTag { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public DateTime? WarrantyExpiryDate { get; set; }
        public string Notes { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
    }
    
    public class UpdateFixedAssetDto
    {
        public string AssetName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string AssetCategory { get; set; } = string.Empty;
        public string AssetAccountId { get; set; } = string.Empty;
        public string AccumulatedDepreciationAccountId { get; set; } = string.Empty;
        public string DepreciationExpenseAccountId { get; set; } = string.Empty;
        public string DisposalGainLossAccountId { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public string LastModifiedBy { get; set; } = string.Empty;
        public bool ForceUpdate { get; set; } = false;
    }
    
    public class FixedAssetDto
    {
        public string Id { get; set; } = string.Empty;
        public string AssetNumber { get; set; } = string.Empty;
        public string AssetName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string AssetCategory { get; set; } = string.Empty;
        public string AssetAccountId { get; set; } = string.Empty;
        public string AccumulatedDepreciationAccountId { get; set; } = string.Empty;
        public string DepreciationExpenseAccountId { get; set; } = string.Empty;
        public string DisposalGainLossAccountId { get; set; } = string.Empty;
        public DateTime PurchaseDate { get; set; }
        public DateTime InServiceDate { get; set; }
        public decimal PurchaseCost { get; set; }
        public decimal ResidualValue { get; set; }
        public decimal DepreciableAmount { get; set; }
        public int UsefulLifeYears { get; set; }
        public DepreciationMethod DepreciationMethod { get; set; }
        public decimal CurrentBookValue { get; set; }
        public decimal AccumulatedDepreciation { get; set; }
        public DateTime? LastDepreciationDate { get; set; }
        public FixedAssetStatus Status { get; set; }
        public string Location { get; set; } = string.Empty;
        public string AssetTag { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public DateTime? WarrantyExpiryDate { get; set; }
        public string Notes { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime LastModifiedDate { get; set; }
        public string LastModifiedBy { get; set; } = string.Empty;
        public DateTime? DisposalDate { get; set; }
        public string DisposalType { get; set; } = string.Empty;
        public decimal? DisposalAmount { get; set; }
        public decimal? DisposalGainLoss { get; set; }
    }
    
    public class DepreciationScheduleDto
    {
        public string AssetId { get; set; } = string.Empty;
        public string AssetNumber { get; set; } = string.Empty;
        public string AssetName { get; set; } = string.Empty;
        public decimal PurchaseCost { get; set; }
        public decimal ResidualValue { get; set; }
        public decimal DepreciableAmount { get; set; }
        public int UsefulLifeYears { get; set; }
        public DepreciationMethod DepreciationMethod { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime GeneratedAt { get; set; }
        public List<DepreciationScheduleItemDto> ScheduleItems { get; set; } = new List<DepreciationScheduleItemDto>();
    }
    
    public class DepreciationScheduleItemDto
    {
        public int Year { get; set; }
        public DateTime PeriodStartDate { get; set; }
        public DateTime PeriodEndDate { get; set; }
        public decimal DepreciationAmount { get; set; }
        public decimal AccumulatedDepreciation { get; set; }
        public decimal BookValueEndOfPeriod { get; set; }
    }
    
    public class AssetDisposalDto
    {
        public int AssetId { get; set; }
        public string FinancialPeriodId { get; set; } = string.Empty;
        public DateTime DisposalDate { get; set; }
        public string DisposalType { get; set; } = string.Empty;
        public decimal DisposalAmount { get; set; }
        public string CashAccountId { get; set; } = string.Empty;
        public string DisposalGainLossAccountId { get; set; } = string.Empty;
        public bool RecordFinalDepreciation { get; set; } = true;
        public string DisposedBy { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }
    
    public class FixedAssetDisposalDto
    {
        public string AssetId { get; set; } = string.Empty;
        public string AssetNumber { get; set; } = string.Empty;
        public string AssetName { get; set; } = string.Empty;
        public DateTime DisposalDate { get; set; }
        public string DisposalType { get; set; } = string.Empty;
        public decimal DisposalAmount { get; set; }
        public decimal NetBookValueAtDisposal { get; set; }
        public decimal GainLossOnDisposal { get; set; }
        public string DisposedBy { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }
    
    public class FixedAssetReportDto
    {
        public string FinancialPeriodId { get; set; } = string.Empty;
        public string FinancialPeriodName { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; }
        public Dictionary<string, AssetCategoryReportDto> AssetsByCategory { get; set; } = new Dictionary<string, AssetCategoryReportDto>();
        public List<AssetReportDetailDto> AssetDetails { get; set; } = new List<AssetReportDetailDto>();
        public decimal TotalPurchaseCost { get; set; }
        public decimal TotalAccumulatedDepreciation { get; set; }
        public decimal TotalNetBookValue { get; set; }
        public int TotalDisposedAssetsCount { get; set; }
        public decimal TotalDisposalProceeds { get; set; }
        public decimal TotalGainLoss { get; set; }
    }
    
    public class AssetCategoryReportDto
    {
        public string CategoryName { get; set; }
        public int AssetCount { get; set; }
        public decimal TotalPurchaseCost { get; set; }
        public decimal TotalAccumulatedDepreciation { get; set; }
        public decimal TotalNetBookValue { get; set; }
    }
    
    public class AssetReportDetailDto
    {
        public string AssetId { get; set; } = string.Empty;
        public string AssetNumber { get; set; } = string.Empty;
        public string AssetName { get; set; } = string.Empty;
        public string AssetCategory { get; set; } = string.Empty;
        public DateTime PurchaseDate { get; set; }
        public DateTime InServiceDate { get; set; }
        public decimal PurchaseCost { get; set; }
        public decimal AccumulatedDepreciation { get; set; }
        public decimal NetBookValue { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public int UsefulLifeYears { get; set; }
        public string DepreciationMethod { get; set; } = string.Empty;
        public DateTime? DisposalDate { get; set; }
        public string DisposalType { get; set; } = string.Empty;
        public decimal DisposalAmount { get; set; }
        public decimal GainLossOnDisposal { get; set; }
    }
    
    public enum DepreciationMethod
    {
        StraightLine,
        DoubleDecliningBalance,
        SumOfYearsDigits,
        Units
    }
    
    public enum FixedAssetStatus
    {
        Draft,
        Active,
        Inactive,
        UnderMaintenance,
        Disposed
    }

    public class AssetRevaluationDto
    {
        public int AssetId { get; set; }
        public decimal NewValue { get; set; }
        public DateTime RevaluationDate { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }

    public class AssetDepreciationDto
    {
        public int AssetId { get; set; }
        public string FinancialPeriodId { get; set; } = string.Empty;
        public decimal DepreciationAmount { get; set; }
        public DateTime DepreciationDate { get; set; }
        public string Notes { get; set; } = string.Empty;
    }
}

