using System;
using FinTech.Core.Domain.Common;

namespace FinTech.Core.Domain.Entities.Loans
{
    /// <summary>
    /// Commodity loan details - special loan type for agricultural/business commodities
    /// Tracks commodity inventory, pricing, and release schedule
    /// </summary>
    public class CommodityLoan : AuditableEntity
    {
        /// <summary>
        /// Reference to the parent loan
        /// </summary>
        public int LoanId { get; set; }
        public virtual Loan Loan { get; set; }
        
        /// <summary>
        /// Type of commodity: "Rice", "Maize", "Palm Oil", "Cassava", "Cocoa", etc.
        /// </summary>
        public string CommodityType { get; set; }
        
        /// <summary>
        /// Total commodity quantity (in bags, kg, liters, etc.)
        /// </summary>
        public decimal Quantity { get; set; }
        
        /// <summary>
        /// Unit of measurement: "Bag", "Kg", "Liter", "Carton"
        /// </summary>
        public string UnitOfMeasurement { get; set; }
        
        /// <summary>
        /// Unit price at time of purchase (₦)
        /// </summary>
        public decimal UnitPrice { get; set; }
        
        /// <summary>
        /// Total commodity value (₦)
        /// </summary>
        public decimal TotalCommodityValue { get; set; }
        
        /// <summary>
        /// Supplier/vendor name
        /// </summary>
        public string SupplierName { get; set; }
        
        /// <summary>
        /// Supplier contact details
        /// </summary>
        public string SupplierContact { get; set; }
        
        /// <summary>
        /// Date commodity was delivered
        /// </summary>
        public DateTime DeliveryDate { get; set; }
        
        /// <summary>
        /// Warehouse/storage location
        /// </summary>
        public string StorageLocation { get; set; }
        
        /// <summary>
        /// Current commodity status: "InWarehouse", "Partial Release", "Fully Released", "Sold"
        /// </summary>
        public string CommodityStatus { get; set; } = "InWarehouse";
        
        /// <summary>
        /// Quantity already released to member
        /// </summary>
        public decimal QuantityReleased { get; set; } = 0;
        
        /// <summary>
        /// Quantity remaining in warehouse
        /// </summary>
        public decimal QuantityRemaining { get; set; }
        
        /// <summary>
        /// Release schedule: "FullRelease", "Scheduled", "OnDemand"
        /// </summary>
        public string ReleaseSchedule { get; set; } = "OnDemand";
        
        /// <summary>
        /// Scheduled release date (if fixed schedule)
        /// </summary>
        public DateTime? ScheduledReleaseDate { get; set; }
        
        /// <summary>
        /// Insurance coverage on commodity
        /// </summary>
        public bool HasInsurance { get; set; }
        
        /// <summary>
        /// Insurance policy number
        /// </summary>
        public string InsurancePolicyNumber { get; set; }
        
        /// <summary>
        /// Insurance amount covered (₦)
        /// </summary>
        public decimal? InsuranceCoverage { get; set; }
        
        /// <summary>
        /// Commodity inspection date
        /// </summary>
        public DateTime? InspectionDate { get; set; }
        
        /// <summary>
        /// Inspection officer name
        /// </summary>
        public string InspectionOfficer { get; set; }
        
        /// <summary>
        /// Inspection report notes
        /// </summary>
        public string InspectionReport { get; set; }
        
        /// <summary>
        /// Quality rating: "Good", "Fair", "Poor"
        /// </summary>
        public string QualityRating { get; set; }
        
        /// <summary>
        /// Expected commodity shelf life (in days)
        /// </summary>
        public int? ShelfLifeDays { get; set; }
        
        /// <summary>
        /// Commodity expiration date
        /// </summary>
        public DateTime? ExpirationDate { get; set; }
        
        /// <summary>
        /// Market price tracker link (if available)
        /// </summary>
        public string MarketPriceUrl { get; set; }
        
        /// <summary>
        /// Current market price (₦) - updated periodically
        /// </summary>
        public decimal? CurrentMarketPrice { get; set; }
        
        /// <summary>
        /// Price last updated date
        /// </summary>
        public DateTime? PriceLastUpdated { get; set; }
        
        /// <summary>
        /// Notes and special conditions
        /// </summary>
        public string Notes { get; set; }
    }
}
