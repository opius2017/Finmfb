using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinTech.Core.Application.Interfaces.Loans;
using FinTech.Core.Application.Interfaces.Repositories.Loans;
using FinTech.Core.Domain.Entities.Loans;
using Microsoft.Extensions.Logging;

namespace FinTech.Core.Application.Services.Loans
{
    public class LoanCollectionService : ILoanCollectionService
    {
        private readonly ILoanCollectionRepository _loanCollectionRepository;
        private readonly ILoanRepository _loanRepository;
        private readonly ILogger<LoanCollectionService> _logger;

        public LoanCollectionService(
            ILoanCollectionRepository loanCollectionRepository,
            ILoanRepository loanRepository,
            ILogger<LoanCollectionService> logger)
        {
            _loanCollectionRepository = loanCollectionRepository ?? throw new ArgumentNullException(nameof(loanCollectionRepository));
            _loanRepository = loanRepository ?? throw new ArgumentNullException(nameof(loanRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<LoanCollection>> GetAllLoanCollectionsAsync()
        {
            try
            {
                _logger.LogInformation("Getting all loan collections");
                return await _loanCollectionRepository.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all loan collections");
                throw;
            }
        }

        public async Task<LoanCollection> GetLoanCollectionByIdAsync(string id)
        {
            try
            {
                _logger.LogInformation("Getting loan collection with ID: {Id}", id);
                return await _loanCollectionRepository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting loan collection with ID: {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<LoanCollection>> GetLoanCollectionsByLoanIdAsync(string loanId)
        {
            try
            {
                _logger.LogInformation("Getting loan collections for loan ID: {LoanId}", loanId);
                return await _loanCollectionRepository.GetByLoanIdAsync(loanId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting loan collections for loan ID: {LoanId}", loanId);
                throw;
            }
        }

        public async Task<LoanCollection> CreateLoanCollectionAsync(LoanCollection loanCollection)
        {
            try
            {
                _logger.LogInformation("Creating loan collection for loan ID: {LoanId}", loanCollection.LoanId);
                
                // Verify loan exists
                var loan = await _loanRepository.GetByIdAsync(loanCollection.LoanId);
                if (loan == null)
                {
                    throw new ArgumentException($"Loan with ID: {loanCollection.LoanId} not found");
                }
                
                // Set default values
                if (loanCollection.Status == CollectionStatus.New)
                {
                    loanCollection.Status = CollectionStatus.New;
                }
                
                // Add collection
                var createdCollection = await _loanCollectionRepository.AddAsync(loanCollection);
                
                _logger.LogInformation("Loan collection created with ID: {Id}", createdCollection.Id);
                
                return createdCollection;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating loan collection for loan ID: {LoanId}", loanCollection.LoanId);
                throw;
            }
        }

        public async Task<LoanCollection> UpdateLoanCollectionAsync(LoanCollection loanCollection)
        {
            try
            {
                _logger.LogInformation("Updating loan collection with ID: {Id}", loanCollection.Id);
                
                // Get existing collection
                var existingCollection = await _loanCollectionRepository.GetByIdAsync(loanCollection.Id);
                if (existingCollection == null)
                {
                    throw new ArgumentException($"Loan collection with ID: {loanCollection.Id} not found");
                }
                
                // Update collection
                var updatedCollection = await _loanCollectionRepository.UpdateAsync(loanCollection);
                
                _logger.LogInformation("Loan collection updated with ID: {Id}", updatedCollection.Id);
                
                return updatedCollection;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating loan collection with ID: {Id}", loanCollection.Id);
                throw;
            }
        }

        public async Task<LoanCollection> AssignCollectorAsync(string id, string collectorId)
        {
            try
            {
                _logger.LogInformation("Assigning collector to loan collection with ID: {Id}", id);
                
                // Get collection
                var collection = await _loanCollectionRepository.GetByIdAsync(id);
                if (collection == null)
                {
                    throw new ArgumentException($"Loan collection with ID: {id} not found");
                }
                
                // Assign collector
                collection.AssignCollector(collectorId);
                
                // Update collection
                var updatedCollection = await _loanCollectionRepository.UpdateAsync(collection);
                
                _logger.LogInformation("Collector assigned to loan collection with ID: {Id}", id);
                
                return updatedCollection;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning collector to loan collection with ID: {Id}", id);
                throw;
            }
        }

        public async Task<LoanCollection> UpdateCollectionStatusAsync(string id, CollectionStatus status, string notes)
        {
            try
            {
                _logger.LogInformation("Updating status of loan collection with ID: {Id} to {Status}", id, status);
                
                // Get collection
                var collection = await _loanCollectionRepository.GetByIdAsync(id);
                if (collection == null)
                {
                    throw new ArgumentException($"Loan collection with ID: {id} not found");
                }
                
                // Update status
                collection.UpdateStatus(status, notes);
                
                // Update collection
                var updatedCollection = await _loanCollectionRepository.UpdateAsync(collection);
                
                _logger.LogInformation("Status updated for loan collection with ID: {Id}", id);
                
                return updatedCollection;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating status of loan collection with ID: {Id}", id);
                throw;
            }
        }

        public async Task<LoanCollectionAction> AddCollectionActionAsync(string collectionId, LoanCollectionAction action)
        {
            try
            {
                _logger.LogInformation("Adding action to loan collection with ID: {Id}", collectionId);
                
                // Get collection
                var collection = await _loanCollectionRepository.GetByIdAsync(collectionId);
                if (collection == null)
                {
                    throw new ArgumentException($"Loan collection with ID: {collectionId} not found");
                }
                
                // Set default values
                action.CollectionId = collectionId;
                action.ActionDate = DateTime.UtcNow;
                
                // Add action
                var addedAction = await _loanCollectionRepository.AddCollectionActionAsync(action);
                
                _logger.LogInformation("Action added to loan collection with ID: {Id}", collectionId);
                
                return addedAction;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding action to loan collection with ID: {Id}", collectionId);
                throw;
            }
        }

        public async Task<IEnumerable<LoanCollectionAction>> GetCollectionActionsAsync(string collectionId)
        {
            try
            {
                _logger.LogInformation("Getting actions for loan collection with ID: {Id}", collectionId);
                return await _loanCollectionRepository.GetCollectionActionsAsync(collectionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting actions for loan collection with ID: {Id}", collectionId);
                throw;
            }
        }
    }
}
