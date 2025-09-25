using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinTech.Core.Domain.Entities.Loans;

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
        Task<IEnumerable<Loan>> GetAllLoansAsync();
        Task<Loan> GetLoanByIdAsync(string id);
        Task<IEnumerable<Loan>> GetLoansByCustomerIdAsync(string customerId);
        Task<Loan> DisburseLoanAsync(string id, decimal amount, string disbursedTo, string reference, string description);
        Task<LoanTransaction> RecordRepaymentAsync(string loanId, decimal amount, decimal principalAmount, decimal interestAmount, decimal feesAmount, decimal penaltyAmount, string reference, string description);
        Task<LoanTransaction> WriteOffLoanAsync(string id, string reason, string approvedBy);
        Task<Loan> RescheduleLoanAsync(string id, DateTime newEndDate, string reason, string approvedBy);
        Task<IEnumerable<LoanTransaction>> GetLoanTransactionsAsync(string loanId);
        Task<IEnumerable<LoanRepaymentSchedule>> GetLoanRepaymentScheduleAsync(string loanId);
        Task<LoanStatement> GenerateLoanStatementAsync(string loanId, DateTime fromDate, DateTime toDate);
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
