using System;
using FinTech.Core.Domain.Enums.FixedAssets;

namespace FinTech.Core.Application.DTOs.FixedAssets
{
    /// <summary>
    /// Data Transfer Object for Asset information
    /// </summary>
    public class AssetDto
    {
        public Guid Id { get; set; }
        public string AssetNumber { get; set; }
        public string AssetName { get; set; }
        public string Description { get; set; }
        public Guid AssetCategoryId { get; set; }
        public string AssetCategoryName { get; set; }
        public string Location { get; set; }
        public string Department { get; set; }
        public Guid? CustodianId { get; set; }
        public string CustodianName { get; set; }
        public string AssetTag { get; set; }
        public string SerialNumber { get; set; }
        public AssetStatus Status { get; set; }
        public DateTime AcquisitionDate { get; set; }
        public decimal AcquisitionCost { get; set; }
        public decimal CurrentValue { get; set; }
        public decimal BookValue { get; set; }
        public decimal AccumulatedDepreciation { get; set; }
        public bool IsDepreciable { get; set; }
        public DepreciationMethod DepreciationMethod { get; set; }
        public int UsefulLifeYears { get; set; }
        public decimal SalvageValuePercent { get; set; }
        public DateTime? LastMaintenanceDate { get; set; }
        public DateTime? NextMaintenanceDate { get; set; }
        public DateTime? WarrantyExpiryDate { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid? CreatedById { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedById { get; set; }
    }
    
    /// <summary>
    /// Data Transfer Object for creating a new Asset
    /// </summary>
    public class CreateAssetDto
    {
        public string AssetName { get; set; }
        public string Description { get; set; }
        public Guid AssetCategoryId { get; set; }
        public string Location { get; set; }
        public string Department { get; set; }
        public Guid? CustodianId { get; set; }
        public string AssetTag { get; set; }
        public string SerialNumber { get; set; }
        public DateTime AcquisitionDate { get; set; }
        public decimal AcquisitionCost { get; set; }
        public bool IsDepreciable { get; set; }
        public DepreciationMethod DepreciationMethod { get; set; }
        public int UsefulLifeYears { get; set; }
        public decimal SalvageValuePercent { get; set; }
        public DateTime? WarrantyExpiryDate { get; set; }
        public string Notes { get; set; }
    }
    
    /// <summary>
    /// Data Transfer Object for updating an Asset
    /// </summary>
    public class UpdateAssetDto
    {
        public Guid Id { get; set; }
        public string AssetName { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public string Department { get; set; }
        public Guid? CustodianId { get; set; }
        public string AssetTag { get; set; }
        public string SerialNumber { get; set; }
        public string Notes { get; set; }
        public DateTime? WarrantyExpiryDate { get; set; }
    }
    
    /// <summary>
    /// Data Transfer Object for Asset Category information
    /// </summary>
    public class AssetCategoryDto
    {
        public Guid Id { get; set; }
        public string CategoryCode { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }
        public Guid? ParentCategoryId { get; set; }
        public string ParentCategoryName { get; set; }
        public DepreciationMethod DefaultDepreciationMethod { get; set; }
        public int DefaultUsefulLifeYears { get; set; }
        public decimal DefaultSalvageValuePercent { get; set; }
        public Guid? AssetAccountId { get; set; }
        public Guid? DepreciationExpenseAccountId { get; set; }
        public Guid? AccumulatedDepreciationAccountId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
    
    /// <summary>
    /// Data Transfer Object for creating a new Asset Category
    /// </summary>
    public class CreateAssetCategoryDto
    {
        public string CategoryCode { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }
        public Guid? ParentCategoryId { get; set; }
        public DepreciationMethod DefaultDepreciationMethod { get; set; }
        public int DefaultUsefulLifeYears { get; set; }
        public decimal DefaultSalvageValuePercent { get; set; }
        public Guid? AssetAccountId { get; set; }
        public Guid? DepreciationExpenseAccountId { get; set; }
        public Guid? AccumulatedDepreciationAccountId { get; set; }
    }
    
    /// <summary>
    /// Data Transfer Object for updating an Asset Category
    /// </summary>
    public class UpdateAssetCategoryDto
    {
        public Guid Id { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }
        public Guid? ParentCategoryId { get; set; }
        public DepreciationMethod DefaultDepreciationMethod { get; set; }
        public int DefaultUsefulLifeYears { get; set; }
        public decimal DefaultSalvageValuePercent { get; set; }
        public Guid? AssetAccountId { get; set; }
        public Guid? DepreciationExpenseAccountId { get; set; }
        public Guid? AccumulatedDepreciationAccountId { get; set; }
    }
    
    /// <summary>
    /// Data Transfer Object for Asset Depreciation Schedule information
    /// </summary>
    public class AssetDepreciationScheduleDto
    {
        public Guid Id { get; set; }
        public Guid AssetId { get; set; }
        public string AssetNumber { get; set; }
        public string AssetName { get; set; }
        public int PeriodNumber { get; set; }
        public DateTime PeriodStartDate { get; set; }
        public DateTime PeriodEndDate { get; set; }
        public decimal DepreciationAmount { get; set; }
        public decimal BookValueBeforeDepreciation { get; set; }
        public decimal BookValueAfterDepreciation { get; set; }
        public bool IsPosted { get; set; }
        public DateTime? PostedDate { get; set; }
        public bool PostedToGL { get; set; }
        public DateTime? PostedToGLDate { get; set; }
    }
    
    /// <summary>
    /// Data Transfer Object for Asset Maintenance information
    /// </summary>
    public class AssetMaintenanceDto
    {
        public Guid Id { get; set; }
        public string MaintenanceNumber { get; set; }
        public Guid AssetId { get; set; }
        public string AssetNumber { get; set; }
        public string AssetName { get; set; }
        public DateTime MaintenanceDate { get; set; }
        public MaintenanceType MaintenanceType { get; set; }
        public MaintenanceStatus Status { get; set; }
        public string Description { get; set; }
        public Guid? VendorId { get; set; }
        public string VendorName { get; set; }
        public decimal Cost { get; set; }
        public string Notes { get; set; }
        public DateTime? CompletionDate { get; set; }
        public Guid? CompletedById { get; set; }
    }
    
    /// <summary>
    /// Data Transfer Object for creating a new Asset Maintenance record
    /// </summary>
    public class CreateAssetMaintenanceDto
    {
        public Guid AssetId { get; set; }
        public DateTime MaintenanceDate { get; set; }
        public MaintenanceType MaintenanceType { get; set; }
        public string Description { get; set; }
        public Guid? VendorId { get; set; }
        public decimal Cost { get; set; }
        public string Notes { get; set; }
    }
    
    /// <summary>
    /// Data Transfer Object for updating an Asset Maintenance record
    /// </summary>
    public class UpdateAssetMaintenanceDto
    {
        public Guid Id { get; set; }
        public DateTime MaintenanceDate { get; set; }
        public MaintenanceType MaintenanceType { get; set; }
        public MaintenanceStatus Status { get; set; }
        public string Description { get; set; }
        public Guid? VendorId { get; set; }
        public decimal Cost { get; set; }
        public string Notes { get; set; }
    }
}
