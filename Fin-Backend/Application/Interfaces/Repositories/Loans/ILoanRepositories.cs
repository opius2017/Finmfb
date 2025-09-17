using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinTech.Domain.Entities.Loans;

namespace FinTech.Application.Interfaces.Repositories.Loans
{
    /// <summary>
    /// Interface for loan application repository
    /// </summary>
    public interface ILoanApplicationRepository
    {
        Task<IEnumerable<LoanApplication>> GetAllAsync();
        Task<LoanApplication> GetByIdAsync(string id);
        Task<IEnumerable<LoanApplication>> GetByCustomerIdAsync(string customerId);
        Task<LoanApplication> AddAsync(LoanApplication loanApplication);
        Task<LoanApplication> UpdateAsync(LoanApplication loanApplication);
        Task<bool> DeleteAsync(string id);
    }
    
    /// <summary>
    /// Interface for loan repository
    /// </summary>
    public interface ILoanRepository
    {
        Task<IEnumerable<Loan>> GetAllAsync();
        Task<Loan> GetByIdAsync(string id);
        Task<IEnumerable<Loan>> GetByCustomerIdAsync(string customerId);
        Task<Loan> AddAsync(Loan loan);
        Task<Loan> UpdateAsync(Loan loan);
        Task<bool> DeleteAsync(string id);
        Task<IEnumerable<LoanTransaction>> GetLoanTransactionsAsync(string loanId);
        Task<IEnumerable<LoanRepaymentSchedule>> GetLoanRepaymentScheduleAsync(string loanId);
    }
    
    /// <summary>
    /// Interface for loan product repository
    /// </summary>
    public interface ILoanProductRepository
    {
        Task<IEnumerable<LoanProduct>> GetAllAsync();
        Task<LoanProduct> GetByIdAsync(string id);
        Task<LoanProduct> AddAsync(LoanProduct loanProduct);
        Task<LoanProduct> UpdateAsync(LoanProduct loanProduct);
        Task<bool> DeleteAsync(string id);
    }
    
    /// <summary>
    /// Interface for loan transaction repository
    /// </summary>
    public interface ILoanTransactionRepository
    {
        Task<IEnumerable<LoanTransaction>> GetAllAsync();
        Task<LoanTransaction> GetByIdAsync(string id);
        Task<IEnumerable<LoanTransaction>> GetByLoanIdAsync(string loanId);
        Task<LoanTransaction> AddAsync(LoanTransaction transaction);
        Task<LoanTransaction> UpdateAsync(LoanTransaction transaction);
    }
    
    /// <summary>
    /// Interface for loan repayment schedule repository
    /// </summary>
    public interface ILoanRepaymentScheduleRepository
    {
        Task<IEnumerable<LoanRepaymentSchedule>> GetAllAsync();
        Task<LoanRepaymentSchedule> GetByIdAsync(string id);
        Task<IEnumerable<LoanRepaymentSchedule>> GetByLoanIdAsync(string loanId);
        Task<LoanRepaymentSchedule> AddAsync(LoanRepaymentSchedule schedule);
        Task<LoanRepaymentSchedule> UpdateAsync(LoanRepaymentSchedule schedule);
        Task<bool> DeleteAsync(string id);
    }
    
    /// <summary>
    /// Interface for loan collection repository
    /// </summary>
    public interface ILoanCollectionRepository
    {
        Task<IEnumerable<LoanCollection>> GetAllAsync();
        Task<LoanCollection> GetByIdAsync(string id);
        Task<IEnumerable<LoanCollection>> GetByLoanIdAsync(string loanId);
        Task<LoanCollection> AddAsync(LoanCollection collection);
        Task<LoanCollection> UpdateAsync(LoanCollection collection);
        Task<bool> DeleteAsync(string id);
        Task<IEnumerable<LoanCollectionAction>> GetCollectionActionsAsync(string collectionId);
        Task<LoanCollectionAction> AddCollectionActionAsync(LoanCollectionAction action);
    }
    
    /// <summary>
    /// Interface for loan document repository
    /// </summary>
    public interface ILoanDocumentRepository
    {
        Task<IEnumerable<LoanDocument>> GetAllAsync();
        Task<LoanDocument> GetByIdAsync(string id);
        Task<IEnumerable<LoanDocument>> GetByLoanIdAsync(string loanId);
        Task<LoanDocument> AddAsync(LoanDocument document);
        Task<LoanDocument> UpdateAsync(LoanDocument document);
        Task<bool> DeleteAsync(string id);
        Task<byte[]> GetDocumentFileAsync(string id);
        Task SaveDocumentFileAsync(string id, byte[] fileData);
    }
    
    /// <summary>
    /// Interface for loan collateral repository
    /// </summary>
    public interface ILoanCollateralRepository
    {
        Task<IEnumerable<LoanCollateral>> GetAllAsync();
        Task<LoanCollateral> GetByIdAsync(string id);
        Task<IEnumerable<LoanCollateral>> GetByLoanIdAsync(string loanId);
        Task<LoanCollateral> AddAsync(LoanCollateral collateral);
        Task<LoanCollateral> UpdateAsync(LoanCollateral collateral);
        Task<bool> DeleteAsync(string id);
    }
    
    /// <summary>
    /// Interface for loan guarantor repository
    /// </summary>
    public interface ILoanGuarantorRepository
    {
        Task<IEnumerable<LoanGuarantor>> GetAllAsync();
        Task<LoanGuarantor> GetByIdAsync(string id);
        Task<IEnumerable<LoanGuarantor>> GetByLoanIdAsync(string loanId);
        Task<LoanGuarantor> AddAsync(LoanGuarantor guarantor);
        Task<LoanGuarantor> UpdateAsync(LoanGuarantor guarantor);
        Task<bool> DeleteAsync(string id);
    }
}