namespace FinTech.Core.Application.Interfaces.Services;

public interface ILoanCollateralService
{
    Task<Guid> AddCollateralAsync(Guid loanId, string collateralType, string description, decimal value, string? documentPath = null, CancellationToken cancellationToken = default);
    Task<bool> UpdateCollateralAsync(Guid collateralId, string description, decimal value, string? documentPath = null, CancellationToken cancellationToken = default);
    Task<bool> RemoveCollateralAsync(Guid collateralId, CancellationToken cancellationToken = default);
    Task<IEnumerable<object>> GetLoanCollateralsAsync(Guid loanId, CancellationToken cancellationToken = default);
    Task<object?> GetCollateralByIdAsync(Guid collateralId, CancellationToken cancellationToken = default);
    Task<bool> ReleaseCollateralAsync(Guid collateralId, string releasedBy, string? notes = null, CancellationToken cancellationToken = default);
}
