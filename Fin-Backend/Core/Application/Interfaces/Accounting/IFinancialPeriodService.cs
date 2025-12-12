using FinTech.Core.Application.DTOs.Accounting;
using FinTech.Core.Application.Common;
using FinTech.Core.Application.Common.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FinTech.Core.Application.Interfaces.Accounting
{
    public interface IFinancialPeriodService
    {
        Task<List<FinancialPeriodDto>> GetFinancialPeriodsByYearAsync(int fiscalYear);
        Task<FinancialPeriodDto> GetFinancialPeriodByIdAsync(string id);
        Task<BaseResponse<PeriodClosingStatusDto>> ClosePeriodAsync(ClosePeriodRequestDto request);
        Task<PeriodClosingSummaryDto> GetPeriodClosingSummaryAsync(string id);
    }
}
