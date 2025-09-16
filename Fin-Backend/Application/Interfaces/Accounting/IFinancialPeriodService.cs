using FinTech.WebAPI.Application.DTOs.Accounting;
using FinTech.WebAPI.Application.DTOs.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FinTech.WebAPI.Application.Interfaces.Accounting
{
    public interface IFinancialPeriodService
    {
        Task<List<FinancialPeriodDto>> GetFinancialPeriodsByYearAsync(int fiscalYear);
        Task<FinancialPeriodDto> GetFinancialPeriodByIdAsync(string id);
        Task<BaseResponse<PeriodClosingStatusDto>> ClosePeriodAsync(ClosePeriodRequestDto request);
        Task<PeriodClosingSummaryDto> GetPeriodClosingSummaryAsync(string id);
    }
}
