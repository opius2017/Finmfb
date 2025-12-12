using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinTech.Core.Domain.Entities.Loans;
using FinTech.Core.Application.DTOs.Loans;

namespace FinTech.Core.Application.Interfaces.Loans
{
    /// <summary>
    /// Interface for loan application service
    /// </summary>
    public interface ILoanApplicationService
    {
        Task<IEnumerable<LoanApplication>> GetAllLoanApplicationsAsync();
        Task<LoanApplication> GetLoanApplicationByIdAsync(string id);
        Task<IEnumerable<LoanApplication>> GetLoanApplicationsByCustomerIdAsync(string customerId);
        Task<LoanApplication> CreateLoanApplicationAsync(LoanApplication loanApplication);
        Task<LoanApplication> UpdateLoanApplicationAsync(LoanApplication loanApplication);
        Task<LoanApplication> SubmitLoanApplicationAsync(string id);
        Task<LoanApplication> ApproveLoanApplicationAsync(string id, string approvedBy, decimal? approvedAmount = null, int? approvedTerm = null);
        Task<LoanApplication> RejectLoanApplicationAsync(string id, string rejectionReason);
        Task<Loan> CreateLoanFromApplicationAsync(string applicationId);
        Task<bool> DeleteLoanApplicationAsync(string id);
    }
    
    /// <summary>
    /// Interface for loan service
    /// </summary>
    public interface ILoanService
    {
        Task<LoanAccount> CreateLoanAccountAsync(CreateLoanAccountRequest request);
        Task<bool> DisburseLoanAsync(Guid loanAccountId, decimal amount, string disbursedBy);
        Task<bool> ProcessRepaymentAsync(Guid loanAccountId, decimal amount, string processedBy);
        Task<List<LoanRepaymentSchedule>> GenerateRepaymentScheduleAsync(Guid loanAccountId);
        Task<bool> ClassifyLoansAsync(Guid tenantId);
        Task<decimal> CalculateProvisionAsync(Guid loanAccountId);
        Task<IEnumerable<LoanCollateral>> GetLoanCollateralsAsync(string loanId);
        Task<LoanCollateral> AddLoanCollateralAsync(string loanId, CreateLoanCollateralDto collateralDto);
    }
    
    /// <summary>
    /// Interface for loan product service
    /// </summary>
    public interface ILoanProductService
    {
        Task<IEnumerable<LoanProduct>> GetAllLoanProductsAsync();
        Task<LoanProduct> GetLoanProductByIdAsync(string id);
        Task<LoanProduct> CreateLoanProductAsync(LoanProduct loanProduct);
        Task<LoanProduct> UpdateLoanProductAsync(LoanProduct loanProduct);
        Task<bool> ActivateLoanProductAsync(string id);
        Task<bool> DeactivateLoanProductAsync(string id);
        Task<bool> DeleteLoanProductAsync(string id);
    }
    
    /// <summary>
    /// Interface for loan collection service
    /// </summary>
    public interface ILoanCollectionService
    {
        Task<IEnumerable<LoanCollection>> GetAllLoanCollectionsAsync();
        Task<LoanCollection> GetLoanCollectionByIdAsync(string id);
        Task<IEnumerable<LoanCollection>> GetLoanCollectionsByLoanIdAsync(string loanId);
        Task<LoanCollection> CreateLoanCollectionAsync(LoanCollection loanCollection);
        Task<LoanCollection> UpdateLoanCollectionAsync(LoanCollection loanCollection);
        Task<LoanCollection> AssignCollectorAsync(string id, string collectorId);
        Task<LoanCollection> UpdateCollectionStatusAsync(string id, CollectionStatus status, string notes);
        Task<LoanCollectionAction> AddCollectionActionAsync(string collectionId, LoanCollectionAction action);
        Task<IEnumerable<LoanCollectionAction>> GetCollectionActionsAsync(string collectionId);
    }
    
    /// <summary>
    /// Interface for loan document service
    /// </summary>
    public interface ILoanDocumentService
    {
        Task<IEnumerable<LoanDocument>> GetLoanDocumentsAsync(string loanId);
        Task<LoanDocument> GetLoanDocumentByIdAsync(string id);
        Task<LoanDocument> UploadLoanDocumentAsync(LoanDocument document, byte[] fileData);
        Task<byte[]> DownloadLoanDocumentAsync(string id);
        Task<bool> VerifyLoanDocumentAsync(string id, string verifiedBy);
        Task<bool> DeleteLoanDocumentAsync(string id);
    }
}
