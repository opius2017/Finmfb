using System;
using System.IO;
using System.Threading.Tasks;
using FinTech.Application.Interfaces.Loans;
using FinTech.Application.Interfaces.Repositories.Loans;
using FinTech.Domain.Entities.Loans;
using Microsoft.Extensions.Logging;

namespace FinTech.Application.Services.Loans
{
    public class LoanDocumentService : ILoanDocumentService
    {
        private readonly ILoanDocumentRepository _loanDocumentRepository;
        private readonly ILogger<LoanDocumentService> _logger;

        public LoanDocumentService(
            ILoanDocumentRepository loanDocumentRepository,
            ILogger<LoanDocumentService> logger)
        {
            _loanDocumentRepository = loanDocumentRepository ?? throw new ArgumentNullException(nameof(loanDocumentRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<LoanDocument>> GetLoanDocumentsAsync(string loanId)
        {
            try
            {
                _logger.LogInformation("Getting documents for loan ID: {LoanId}", loanId);
                return await _loanDocumentRepository.GetByLoanIdAsync(loanId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting documents for loan ID: {LoanId}", loanId);
                throw;
            }
        }

        public async Task<LoanDocument> GetLoanDocumentByIdAsync(string id)
        {
            try
            {
                _logger.LogInformation("Getting document with ID: {Id}", id);
                return await _loanDocumentRepository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting document with ID: {Id}", id);
                throw;
            }
        }

        public async Task<LoanDocument> UploadLoanDocumentAsync(LoanDocument document, byte[] fileData)
        {
            try
            {
                _logger.LogInformation("Uploading document for loan ID: {LoanId}", document.LoanId);
                
                // Set upload metadata
                document.UploadDate = DateTime.UtcNow;
                document.Status = DocumentStatus.Pending;
                
                // Add document to database
                var addedDocument = await _loanDocumentRepository.AddAsync(document);
                
                // Save file data
                await _loanDocumentRepository.SaveDocumentFileAsync(addedDocument.Id, fileData);
                
                _logger.LogInformation("Document uploaded with ID: {Id} for loan ID: {LoanId}", 
                    addedDocument.Id, document.LoanId);
                
                return addedDocument;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading document for loan ID: {LoanId}", document.LoanId);
                throw;
            }
        }

        public async Task<byte[]> DownloadLoanDocumentAsync(string id)
        {
            try
            {
                _logger.LogInformation("Downloading document with ID: {Id}", id);
                
                // Verify document exists
                var document = await _loanDocumentRepository.GetByIdAsync(id);
                if (document == null)
                {
                    throw new ArgumentException($"Document with ID: {id} not found");
                }
                
                // Get file data
                var fileData = await _loanDocumentRepository.GetDocumentFileAsync(id);
                if (fileData == null || fileData.Length == 0)
                {
                    throw new FileNotFoundException($"File data for document with ID: {id} not found");
                }
                
                return fileData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading document with ID: {Id}", id);
                throw;
            }
        }

        public async Task<bool> VerifyLoanDocumentAsync(string id, string verifiedBy)
        {
            try
            {
                _logger.LogInformation("Verifying document with ID: {Id}", id);
                
                // Get document
                var document = await _loanDocumentRepository.GetByIdAsync(id);
                if (document == null)
                {
                    throw new ArgumentException($"Document with ID: {id} not found");
                }
                
                // Update document
                document.Status = DocumentStatus.Verified;
                document.VerificationDate = DateTime.UtcNow;
                document.VerifiedBy = verifiedBy;
                
                // Save changes
                await _loanDocumentRepository.UpdateAsync(document);
                
                _logger.LogInformation("Document verified with ID: {Id}", id);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying document with ID: {Id}", id);
                throw;
            }
        }

        public async Task<bool> DeleteLoanDocumentAsync(string id)
        {
            try
            {
                _logger.LogInformation("Deleting document with ID: {Id}", id);
                return await _loanDocumentRepository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting document with ID: {Id}", id);
                throw;
            }
        }
    }
}