using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FinTech.Core.Application.Interfaces.Services;
using FinTech.Domain.Entities.Accounting;
using FinTech.Domain.Repositories.Accounting;

namespace FinTech.Core.Application.Services
{
    /// <summary>
    /// Service implementation for Chart of Accounts management
    /// </summary>
    public class ChartOfAccountService : IChartOfAccountService
    {
        private readonly IChartOfAccountRepository _chartOfAccountRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ChartOfAccountService(IChartOfAccountRepository chartOfAccountRepository, IUnitOfWork unitOfWork)
        {
            _chartOfAccountRepository = chartOfAccountRepository ?? throw new ArgumentNullException(nameof(chartOfAccountRepository));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<ChartOfAccount> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            return await _chartOfAccountRepository.GetByIdAsync(id, cancellationToken);
        }

        public async Task<ChartOfAccount> GetByAccountNumberAsync(string accountNumber, CancellationToken cancellationToken = default)
        {
            return await _chartOfAccountRepository.GetByAccountNumberAsync(accountNumber, cancellationToken);
        }

        public async Task<IReadOnlyList<ChartOfAccount>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _chartOfAccountRepository.GetAllAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<ChartOfAccount>> GetByTypeAsync(AccountType accountType, CancellationToken cancellationToken = default)
        {
            return await _chartOfAccountRepository.GetByTypeAsync(accountType, cancellationToken);
        }

        public async Task<IReadOnlyList<ChartOfAccount>> GetByClassificationAsync(AccountClassification classification, CancellationToken cancellationToken = default)
        {
            return await _chartOfAccountRepository.GetByClassificationAsync(classification, cancellationToken);
        }

        public async Task<IReadOnlyList<ChartOfAccount>> GetActiveAccountsAsync(CancellationToken cancellationToken = default)
        {
            return await _chartOfAccountRepository.GetActiveAccountsAsync(cancellationToken);
        }

        public async Task<string> CreateAccountAsync(ChartOfAccount account, CancellationToken cancellationToken = default)
        {
            // Validate the account
            if (string.IsNullOrWhiteSpace(account.AccountNumber))
            {
                throw new ArgumentException("Account number is required");
            }

            if (string.IsNullOrWhiteSpace(account.AccountName))
            {
                throw new ArgumentException("Account name is required");
            }

            // Check if account number already exists
            if (await AccountNumberExistsAsync(account.AccountNumber, cancellationToken))
            {
                throw new InvalidOperationException($"Account number {account.AccountNumber} already exists");
            }

            // Set default values if not provided
            if (string.IsNullOrEmpty(account.Id))
            {
                account.Id = Guid.NewGuid().ToString();
            }

            account.CreatedAt = DateTime.UtcNow;
            account.Status = account.Status == AccountStatus.Undefined ? AccountStatus.Active : account.Status;

            // Add the account
            await _chartOfAccountRepository.AddAsync(account, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return account.Id;
        }

        public async Task UpdateAccountAsync(ChartOfAccount account, CancellationToken cancellationToken = default)
        {
            // Validate the account
            if (string.IsNullOrWhiteSpace(account.AccountNumber))
            {
                throw new ArgumentException("Account number is required");
            }

            if (string.IsNullOrWhiteSpace(account.AccountName))
            {
                throw new ArgumentException("Account name is required");
            }

            // Get the existing account
            var existingAccount = await _chartOfAccountRepository.GetByIdAsync(account.Id, cancellationToken);
            if (existingAccount == null)
            {
                throw new InvalidOperationException($"Account with ID {account.Id} not found");
            }

            // Check if account number changed and if the new number already exists
            if (existingAccount.AccountNumber != account.AccountNumber &&
                await AccountNumberExistsAsync(account.AccountNumber, cancellationToken))
            {
                throw new InvalidOperationException($"Account number {account.AccountNumber} already exists");
            }

            // Update the account
            account.LastModifiedAt = DateTime.UtcNow;
            await _chartOfAccountRepository.UpdateAsync(account, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task ActivateAccountAsync(string id, string modifiedBy, CancellationToken cancellationToken = default)
        {
            var account = await _chartOfAccountRepository.GetByIdAsync(id, cancellationToken);
            if (account == null)
            {
                throw new InvalidOperationException($"Account with ID {id} not found");
            }

            account.Status = AccountStatus.Active;
            account.LastModifiedBy = modifiedBy;
            account.LastModifiedAt = DateTime.UtcNow;

            await _chartOfAccountRepository.UpdateAsync(account, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task DeactivateAccountAsync(string id, string modifiedBy, CancellationToken cancellationToken = default)
        {
            var account = await _chartOfAccountRepository.GetByIdAsync(id, cancellationToken);
            if (account == null)
            {
                throw new InvalidOperationException($"Account with ID {id} not found");
            }

            account.Status = AccountStatus.Inactive;
            account.LastModifiedBy = modifiedBy;
            account.LastModifiedAt = DateTime.UtcNow;

            await _chartOfAccountRepository.UpdateAsync(account, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task<bool> AccountNumberExistsAsync(string accountNumber, CancellationToken cancellationToken = default)
        {
            var account = await _chartOfAccountRepository.GetByAccountNumberAsync(accountNumber, cancellationToken);
            return account != null;
        }

        public async Task<string> GenerateAccountNumberAsync(AccountType accountType, AccountClassification classification, CancellationToken cancellationToken = default)
        {
            // Get the prefix based on account type and classification
            string prefix = GetAccountNumberPrefix(accountType, classification);

            // Get the highest account number with this prefix
            var highestAccount = await _chartOfAccountRepository.GetHighestAccountNumberByPrefixAsync(prefix, cancellationToken);

            // Extract the sequence number from the highest account number
            int sequence = 1;
            if (highestAccount != null)
            {
                string sequencePart = highestAccount.AccountNumber.Substring(prefix.Length);
                if (int.TryParse(sequencePart, out int lastSequence))
                {
                    sequence = lastSequence + 1;
                }
            }

            // Format: [Prefix][Sequence]
            // For example: "1001", "2001", "3001" etc.
            return $"{prefix}{sequence:D4}";
        }

        private string GetAccountNumberPrefix(AccountType accountType, AccountClassification classification)
        {
            // First digit based on account type
            string typeDigit;
            switch (accountType)
            {
                case AccountType.Asset:
                    typeDigit = "1";
                    break;
                case AccountType.Liability:
                    typeDigit = "2";
                    break;
                case AccountType.Equity:
                    typeDigit = "3";
                    break;
                case AccountType.Revenue:
                    typeDigit = "4";
                    break;
                case AccountType.Expense:
                    typeDigit = "5";
                    break;
                default:
                    typeDigit = "9"; // Other/Unknown
                    break;
            }

            // Second and third digits based on classification
            string classDigits;
            switch (classification)
            {
                case AccountClassification.CurrentAsset:
                    classDigits = "10";
                    break;
                case AccountClassification.NonCurrentAsset:
                    classDigits = "20";
                    break;
                case AccountClassification.CurrentLiability:
                    classDigits = "10";
                    break;
                case AccountClassification.NonCurrentLiability:
                    classDigits = "20";
                    break;
                case AccountClassification.OwnerEquity:
                    classDigits = "10";
                    break;
                case AccountClassification.RetainedEarnings:
                    classDigits = "20";
                    break;
                case AccountClassification.OperatingRevenue:
                    classDigits = "10";
                    break;
                case AccountClassification.NonOperatingRevenue:
                    classDigits = "20";
                    break;
                case AccountClassification.OperatingExpense:
                    classDigits = "10";
                    break;
                case AccountClassification.NonOperatingExpense:
                    classDigits = "20";
                    break;
                default:
                    classDigits = "00";
                    break;
            }

            return typeDigit + classDigits;
        }
    }
}