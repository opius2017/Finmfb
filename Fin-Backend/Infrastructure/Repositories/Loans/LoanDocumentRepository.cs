using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FinTech.Core.Application.Interfaces.Repositories.Loans;
using FinTech.Core.Domain.Entities.Loans;
using FinTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinTech.Infrastructure.Repositories.Loans
{
    public class LoanDocumentRepository : ILoanDocumentRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LoanDocumentRepository> _logger;
        private readonly string _basePath;

        public LoanDocumentRepository(
            ApplicationDbContext context,
            ILogger<LoanDocumentRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Documents", "Loans");
            
            // Ensure directory exists
            if (!Directory.Exists(_basePath))
            {
                Directory.CreateDirectory(_basePath);
            }
        }

        public async Task<IEnumerable<LoanDocument>> GetAllAsync()
        {
            try
            {
                return await _context.LoanDocuments
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all loan documents");
                throw;
            }
        }

        public async Task<LoanDocument> GetByIdAsync(string id)
        {
            try
            {
                return await _context.LoanDocuments
                    .FirstOrDefaultAsync(ld => ld.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting loan document with ID: {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<LoanDocument>> GetByLoanIdAsync(string loanId)
        {
            try
            {
                return await _context.LoanDocuments
                    .Where(ld => ld.LoanId == loanId)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting documents for loan ID: {LoanId}", loanId);
                throw;
            }
        }

        public async Task<LoanDocument> AddAsync(LoanDocument document)
        {
            try
            {
                _context.LoanDocuments.Add(document);
                await _context.SaveChangesAsync();
                return document;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding loan document");
                throw;
            }
        }

        public async Task<LoanDocument> UpdateAsync(LoanDocument document)
        {
            try
            {
                _context.Entry(document).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return document;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating loan document with ID: {Id}", document.Id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(string id)
        {
            try
            {
                var document = await _context.LoanDocuments.FindAsync(id);
                if (document == null)
                {
                    return false;
                }

                // Delete file
                string filePath = GetFilePath(id);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                _context.LoanDocuments.Remove(document);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting loan document with ID: {Id}", id);
                throw;
            }
        }

        public async Task<byte[]> GetDocumentFileAsync(string id)
        {
            try
            {
                string filePath = GetFilePath(id);
                if (!File.Exists(filePath))
                {
                    return null;
                }

                return await File.ReadAllBytesAsync(filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting document file with ID: {Id}", id);
                throw;
            }
        }

        public async Task SaveDocumentFileAsync(string id, byte[] fileData)
        {
            try
            {
                string filePath = GetFilePath(id);
                await File.WriteAllBytesAsync(filePath, fileData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving document file with ID: {Id}", id);
                throw;
            }
        }

        private string GetFilePath(string documentId)
        {
            return Path.Combine(_basePath, documentId);
        }
    }
}
