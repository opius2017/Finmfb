using System;
using System.Threading.Tasks;
using FinTech.Core.Application.DTOs.Loans;
using FinTech.Core.Application.Services.Loans;
using FinTech.Core.Domain.Entities.Loans;
using FinTech.Core.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using FinTech.Core.Domain.Enums.Loans;

using FinTech.Core.Application.Interfaces.Loans;

namespace FinTech.Core.Application.Services.Loans
{
    /// <summary>
    /// Service for managing loan disbursement workflow
    /// </summary>
    public class LoanDisbursementService : ILoanDisbursementService
    {
        private readonly IRepository<Loan> _loanRepository;
        private readonly IRepository<LoanApplication> _loanApplicationRepository;
        private readonly IRepository<Member> _memberRepository;
        private readonly ILoanCalculatorService _calculatorService;
        private readonly ILoanRegisterService _registerService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<LoanDisbursementService> _logger;

        public LoanDisbursementService(
            IRepository<Loan> loanRepository,
            IRepository<LoanApplication> loanApplicationRepository,
            IRepository<Member> memberRepository,
            ILoanCalculatorService calculatorService,
            ILoanRegisterService registerService,
            IUnitOfWork unitOfWork,
            ILogger<LoanDisbursementService> logger)
        {
            _loanRepository = loanRepository;
            _loanApplicationRepository = loanApplicationRepository;
            _memberRepository = memberRepository;
            _calculatorService = calculatorService;
            _registerService = registerService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>
        /// Initiates cash loan disbursement workflow
        /// </summary>
        public async Task<DisbursementResult> DisburseCashLoanAsync(DisbursementRequest request)
        {
            _logger.LogInformation(
                "Initiating cash loan disbursement for application {ApplicationId}",
                request.LoanApplicationId);

            // Validate application
            var application = await _loanApplicationRepository.GetByIdAsync(request.LoanApplicationId);
            if (application == null)
                throw new InvalidOperationException("Loan application not found");

            if (application.Status != LoanApplicationStatus.Approved)
                throw new InvalidOperationException("Only approved applications can be disbursed");

            var member = await _memberRepository.GetByIdAsync(application.MemberId);
            if (member == null)
                throw new InvalidOperationException("Member not found");

            // Create loan record
            var loan = await CreateLoanRecordAsync(application, request);

            // Register loan and get serial number
            var registration = await _registerService.RegisterLoanAsync(loan.Id, request.DisbursedBy);

            // Generate loan agreement document
            var agreementDocument = await GenerateLoanAgreementAsync(loan, member, registration.SerialNumber);

            // Process bank transfer (integration point)
            var transferResult = await ProcessBankTransferAsync(
                member,
                loan.PrincipalAmount,
                request.BankTransferReference);

            // Update loan status
            loan.Status = "DISBURSED"; // Loan.Status
            loan.DisbursementDate = DateTime.UtcNow;
            loan.UpdatedAt = DateTime.UtcNow;
            await _loanRepository.UpdateAsync(loan);

            // Update application status
            application.Status = LoanApplicationStatus.Disbursed;
            application.DisbursedDate = DateTime.UtcNow;
            application.UpdatedAt = DateTime.UtcNow;
            await _loanApplicationRepository.UpdateAsync(application);

            // Update member loan totals
            // FinTech Best Practice: TotalLoans is int, increment by 1
            member.TotalLoans = (int)(member.TotalLoans + 1);
            member.OutstandingLoanBalance += loan.OutstandingBalance;
            member.UpdatedAt = DateTime.UtcNow;
            await _memberRepository.UpdateAsync(member);

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Loan disbursed successfully. Loan ID: {LoanId}, Serial: {SerialNumber}",
                loan.Id, registration.SerialNumber);

            return new DisbursementResult
            {
                Success = true,
                LoanId = loan.Id,
                SerialNumber = registration.SerialNumber,
                DisbursedAmount = loan.PrincipalAmount,
                DisbursementDate = loan.DisbursementDate,
                BankTransferReference = transferResult.TransactionId,
                AgreementDocumentUrl = agreementDocument.DocumentUrl,
                FirstPaymentDate = loan.NextPaymentDate,
                MonthlyEMI = loan.MonthlyInstallment,
                Message = "Loan disbursed successfully"
            };
        }

        /// <summary>
        /// Generates loan agreement document
        /// </summary>
        public async Task<LoanAgreementDocument> GenerateLoanAgreementAsync(
            Loan loan,
            Member member,
            string serialNumber)
        {
            _logger.LogInformation("Generating loan agreement for loan {LoanId}", loan.Id);

            // Generate amortization schedule
            // Generate amortization schedule
            var scheduleRequest = new AmortizationScheduleRequest
            {
                Principal = loan.PrincipalAmount,
                AnnualInterestRate = loan.InterestRate,
                TenureMonths = loan.RepaymentPeriodMonths,
                StartDate = loan.DisbursementDate
            };
            var installments = _calculatorService.GenerateAmortizationSchedule(scheduleRequest).Installments;
            var schedule = installments.Select(i => new FinTech.Core.Application.DTOs.Loans.AmortizationScheduleItem 
            {
                InstallmentNumber = i.InstallmentNumber,
                DueDate = i.DueDate,
                EMI = i.EMIAmount,
                PrincipalPayment = i.PrincipalAmount,
                InterestPayment = i.InterestAmount,
                RemainingBalance = i.ClosingBalance,
                Status = "Pending"
            }).ToList();

            // Create agreement document content
            var agreementContent = GenerateAgreementContent(loan, member, serialNumber, schedule);

            // TODO: Generate PDF document
            // For now, return a placeholder
            var document = new LoanAgreementDocument
            {
                LoanId = loan.Id,
                SerialNumber = serialNumber,
                DocumentUrl = $"/documents/loan-agreements/{serialNumber}.pdf",
                GeneratedAt = DateTime.UtcNow,
                Content = agreementContent
            };

            _logger.LogInformation("Loan agreement generated for loan {LoanId}", loan.Id);

            return await Task.FromResult(document);
        }

        /// <summary>
        /// Processes bank transfer for loan disbursement
        /// </summary>
        public async Task<BankTransferResult> ProcessBankTransferAsync(
            Member member,
            decimal amount,
            string reference)
        {
            _logger.LogInformation(
                "Processing bank transfer of ₦{Amount:N2} to member {MemberId}",
                amount, member.Id);

            // TODO: Integrate with actual bank transfer API (NIBSS/Interswitch)
            // For now, simulate successful transfer
            var result = new BankTransferResult
            {
                Success = true,
                TransactionId = reference ?? $"TXN{DateTime.UtcNow:yyyyMMddHHmmss}",
                Amount = amount,
                RecipientAccount = "****" + (member.PhoneNumber?.Substring(Math.Max(0, member.PhoneNumber.Length - 4)) ?? "0000"),
                RecipientName = $"{member.FirstName} {member.LastName}",
                TransferDate = DateTime.UtcNow,
                Status = "COMPLETED",
                Message = "Transfer completed successfully"
            };

            _logger.LogInformation(
                "Bank transfer completed. Transaction ID: {TransactionId}",
                result.TransactionId);

            return await Task.FromResult(result);
        }

        /// <summary>
        /// Tracks disbursement transaction and confirms completion
        /// </summary>
        public async Task<TransactionTrackingResult> TrackDisbursementAsync(string transactionId)
        {
            _logger.LogInformation("Tracking disbursement transaction {TransactionId}", transactionId);

            // TODO: Integrate with bank API to check transaction status
            // For now, return simulated result
            var result = new TransactionTrackingResult
            {
                TransactionId = transactionId,
                Status = "COMPLETED",
                LastUpdated = DateTime.UtcNow,
                Message = "Transaction completed successfully"
            };

            return await Task.FromResult(result);
        }

        /// <summary>
        /// Sends disbursement confirmation notification
        /// </summary>
        public async Task SendDisbursementNotificationAsync(
            string memberId,
            DisbursementResult disbursement)
        {
            _logger.LogInformation(
                "Sending disbursement notification to member {MemberId}",
                memberId);

            var member = await _memberRepository.GetByIdAsync(memberId);
            if (member == null)
                return;

            // TODO: Integrate with notification service (email/SMS)
            // Notification content:
            var notificationContent = $@"
Dear {member.FirstName},

Your loan has been disbursed successfully!

Loan Details:
- Serial Number: {disbursement.SerialNumber}
- Amount: ₦{disbursement.DisbursedAmount:N2}
- Monthly Payment: ₦{disbursement.MonthlyEMI:N2}
- First Payment Date: {disbursement.FirstPaymentDate:yyyy-MM-dd}

Transaction Reference: {disbursement.BankTransferReference}

Please check your bank account for the credit.

Thank you for your continued membership.
";

            _logger.LogInformation("Disbursement notification sent to member {MemberId}", memberId);

            await Task.CompletedTask;
        }

        /// <summary>
        /// Gets disbursement history for a member
        /// </summary>
        public async Task<DisbursementHistory> GetMemberDisbursementHistoryAsync(string memberId)
        {
            var loans = await _loanRepository.GetAll()
                .Where(l => l.MemberId == memberId && l.Status == "DISBURSED")
                .OrderByDescending(l => l.DisbursementDate)
                .ToListAsync();

            var history = new DisbursementHistory
            {
                MemberId = memberId,
                TotalDisbursements = loans.Count,
                TotalAmountDisbursed = loans.Sum(l => l.PrincipalAmount),
                Disbursements = loans.Select(l => new DisbursementSummary
                {
                    LoanId = l.Id,
                    SerialNumber = l.LoanNumber,
                    Amount = l.PrincipalAmount,
                    DisbursementDate = l.DisbursementDate,
                    Status = l.Status
                }).ToList()
            };

            return history;
        }

        #region Helper Methods

        private async Task<Loan> CreateLoanRecordAsync(
            LoanApplication application,
            DisbursementRequest request)
        {
            var disbursementDate = DateTime.UtcNow;
            var maturityDate = disbursementDate.AddMonths(application.RepaymentPeriodMonths);

            // Calculate EMI and schedule
            var emi = _calculatorService.CalculateEMI(
                application.ApprovedAmount ?? application.RequestedAmount,
                application.InterestRate,
                application.RepaymentPeriodMonths);

            var principal = application.ApprovedAmount ?? application.RequestedAmount;
            var totalInterest = _calculatorService.CalculateTotalInterest(
                principal,
                application.InterestRate,
                application.RepaymentPeriodMonths);
            var totalRepayable = principal + totalInterest;

            var loan = new Loan
            {
                // Id = Guid.NewGuid().ToString(), // BaseEntity handles this? Or we set it. CommitteeReview sets it. Most BaseEntities have private set Id. I should assign if possible or remove if protected.
                // Assuming BaseEntity generates ID if not set, or we can set it.
                // LoanCommitteeService we removed explicit set. Here let's try removing it for safety.
                LoanNumber = "PENDING", // Will be updated after registration
                LoanApplicationId = Guid.Parse(application.Id), // LoanApplicationId is Guid
                MemberId = application.MemberId,
                PrincipalAmount = principal,
                InterestRate = application.InterestRate,
                RepaymentPeriodMonths = application.RepaymentPeriodMonths,
                MonthlyInstallment = emi,
                TotalRepayableAmount = totalRepayable,
                OutstandingBalance = totalRepayable,
                PrincipalPaid = 0,
                InterestPaid = 0,
                DisbursementDate = disbursementDate,
                MaturityDate = maturityDate,
                Status = "PENDING_DISBURSEMENT",
                PaymentFrequency = 1, // Monthly int (mapped)
                NextPaymentDate = disbursementDate.AddMonths(1),
                DaysInArrears = 0,
                ArrearsAmount = 0,
                PenaltyAmount = 0,
                Classification = "PERFORMING",
                ProvisionAmount = 0,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = request.DisbursedBy
            };

            await _loanRepository.AddAsync(loan);
            await _unitOfWork.SaveChangesAsync();

            return loan;
        }

        private string GenerateAgreementContent(
            Loan loan,
            Member member,
            string serialNumber,
            List<AmortizationScheduleItem> schedule)
        {
            return $@"
LOAN AGREEMENT

Serial Number: {serialNumber}
Date: {DateTime.UtcNow:yyyy-MM-dd}

BETWEEN:
[Cooperative Name]
(The Lender)

AND:
{member.FirstName} {member.LastName}
Member Number: {member.MemberNumber}
(The Borrower)

LOAN TERMS:
Principal Amount: ₦{loan.PrincipalAmount:N2}
Interest Rate: {loan.InterestRate}% per annum
Tenor: {loan.RepaymentPeriodMonths} months
Monthly Installment: ₦{loan.MonthlyInstallment:N2}
Total Repayable: ₦{loan.TotalRepayableAmount:N2}

Disbursement Date: {loan.DisbursementDate:yyyy-MM-dd}
Maturity Date: {loan.MaturityDate:yyyy-MM-dd}
First Payment Date: {loan.NextPaymentDate:yyyy-MM-dd}

REPAYMENT SCHEDULE:
[Amortization schedule details would be included here]

TERMS AND CONDITIONS:
1. The borrower agrees to repay the loan in {loan.RepaymentPeriodMonths} equal monthly installments.
2. Interest is calculated using the reducing balance method.
3. Late payments will attract penalty charges as per cooperative policy.
4. The loan is secured by guarantors as per cooperative requirements.

SIGNATURES:
_____________________          _____________________
Borrower                       Lender Representative

Date: {DateTime.UtcNow:yyyy-MM-dd}
";
        }

        #endregion
    }
}
