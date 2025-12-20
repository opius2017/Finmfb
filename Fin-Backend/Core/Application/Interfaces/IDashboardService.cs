using System.Threading;
using System.Threading.Tasks;

namespace FinTech.Core.Application.Interfaces
{
    public interface IDashboardService
    {
        Task<object> GetOverviewAsync(CancellationToken cancellationToken = default);
        Task<object> GetExecutiveDashboardAsync(CancellationToken cancellationToken = default);
        Task<object> GetLoanDashboardAsync(CancellationToken cancellationToken = default);
        Task<object> GetDepositDashboardAsync(CancellationToken cancellationToken = default);
        Task<object> GetInventoryDashboardAsync(CancellationToken cancellationToken = default);
        Task<object> GetPayrollDashboardAsync(CancellationToken cancellationToken = default);
    }
}
