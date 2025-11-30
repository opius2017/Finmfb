using System.Collections.Generic;
using System.Threading.Tasks;
using FinTech.Core.Application.DTOs.Loans;

namespace FinTech.Core.Application.Services.Loans
{
    public interface ILoanRegisterService
    {
        Task<LoanRegisterEntryDto> RegisterLoanAsync(string loanId, string registeredBy);
        Task<string> GenerateSerialNumberAsync(int year);
        Task<LoanRegisterEntryDto> GetRegisterEntryAsync(string loanId);
        Task<List<LoanRegisterEntryDto>> GetRegisterEntriesAsync(int? year = null, int? month = null);
        Task<byte[]> ExportRegisterAsync(int year);
    }
}
