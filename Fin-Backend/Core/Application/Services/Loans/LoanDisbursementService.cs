using System;
using System.Threading.Tasks;
using FinTech.Core.Application.DTOs.Loans;
using FinTech.Core.Application.Services.Loans;
using FinTech.Core.Domain.Entities.Loans;
using FinTech.Core.Domain.Repositories;
using Microsoft.Extensions.Logging;

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

            if (application.ApplicationStatus != "APPROVED")
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
            loan.LoanStatus = "DISBURSED";
            loan.DisbursementDate = DateTime.UtcNow;
            loan.UpdatedAt = DateTime.UtcNow;
            await _loanRepository.UpdateAsync(loan);

            // Update application status
            application.ApplicationStatus = "DISBURSED";
            application.DisbursementDate = DateTime.UtcNow;
            application.UpdatedAt = DateTime.UtcNow;
            await _loanApplicationRepository.UpdateAsync(application);

            // Update member loan totals
            member.TotalLoans += loan.PrincipalAmount;
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
            var schedule = _calculatorService.GenerateAmortizationSchedule(
                loan.PrincipalAmount,
                loan.InterestRate,
                loan.RepaymentPeriodMonths,
                loan.DisbursementDate);

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
            var loans = (await _loanRepository.GetAllAsync())
                .Where(l => l.MemberId == memberId && l.LoanStatus == "DISBURSED")
                .OrderByDescending(l => l.DisbursementDate)
                .ToList();

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
                    Status = l.LoanStatus
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

            var totalRepayable = _calculatorService.CalculateTotalRepayable(
                application.ApprovedAmount ?? application.RequestedAmount,
                application.InterestRate,
                application.RepaymentPeriodMonths);

            var loan = new Loan
            {
                Id = Guid.NewGuid().ToString(),
                LoanNumber = "PENDING", // Will be updated after registration
                LoanApplicationId = application.Id,
                MemberId = application.MemberId,
                PrincipalAmount = application.ApprovedAmount ?? application.RequestedAmount,
                InterestRate = application.InterestRate,
                RepaymentPeriodMonths = application.RepaymentPeriodMonths,
                MonthlyInstallment = emi,
                TotalRepayableAmount = totalRepayable,
                OutstandingBalance = totalRepayable,
                PrincipalPaid = 0,
                InterestPaid = 0,
                DisbursementDate = disbursementDate,
                MaturityDate = maturityDate,
                LoanStatus = "PENDING_DISBURSEMENT",
                PaymentFrequency = "MONTHLY",
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
            System.Collections.Generic.List<AmortizationScheduleItem> schedule)
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
