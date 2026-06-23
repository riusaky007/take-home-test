using Fundo.Applications.WebApi.Dtos;

namespace Fundo.Applications.WebApi.Services
{
    public interface ILoanService
    {
        Task<IReadOnlyList<LoanResponse>> GetAllAsync(CancellationToken cancellationToken = default);

        Task<LoanResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        Task<LoanResponse> CreateAsync(CreateLoanRequest request, CancellationToken cancellationToken = default);

        Task<LoanResponse> ApplyPaymentAsync(Guid id, PaymentRequest request, CancellationToken cancellationToken = default);
    }
}
