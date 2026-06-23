using Fundo.Applications.WebApi.Data;
using Fundo.Applications.WebApi.Dtos;
using Fundo.Applications.WebApi.Models;
using Microsoft.EntityFrameworkCore;

namespace Fundo.Applications.WebApi.Services
{
    public class LoanService : ILoanService
    {
        private readonly LoanDbContext _dbContext;
        private readonly ILogger<LoanService> _logger;

        public LoanService(LoanDbContext dbContext, ILogger<LoanService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<IReadOnlyList<LoanResponse>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var loans = await _dbContext.Loans
                .AsNoTracking()
                .OrderBy(l => l.ApplicantName)
                .ToListAsync(cancellationToken);

            return loans.Select(LoanResponse.FromEntity).ToList();
        }

        public async Task<LoanResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var loan = await _dbContext.Loans
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);

            return loan is null ? null : LoanResponse.FromEntity(loan);
        }

        public async Task<LoanResponse> CreateAsync(CreateLoanRequest request, CancellationToken cancellationToken = default)
        {
            var loan = new Loan
            {
                Id = Guid.NewGuid(),
                Amount = request.Amount,
                CurrentBalance = request.Amount,
                ApplicantName = request.ApplicantName.Trim(),
                Status = LoanStatus.Active
            };

            _dbContext.Loans.Add(loan);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Created loan {LoanId} for {ApplicantName} with amount {Amount}",
                loan.Id, loan.ApplicantName, loan.Amount);

            return LoanResponse.FromEntity(loan);
        }

        public async Task<LoanResponse> ApplyPaymentAsync(Guid id, PaymentRequest request, CancellationToken cancellationToken = default)
        {
            var loan = await _dbContext.Loans.FirstOrDefaultAsync(l => l.Id == id, cancellationToken)
                ?? throw new LoanNotFoundException(id);

            if (loan.Status == LoanStatus.Paid)
            {
                throw new InvalidPaymentException("Loan is already paid in full.");
            }

            if (request.Amount > loan.CurrentBalance)
            {
                throw new InvalidPaymentException(
                    $"Payment of {request.Amount} exceeds the current balance of {loan.CurrentBalance}.");
            }

            loan.CurrentBalance -= request.Amount;

            if (loan.CurrentBalance == 0m)
            {
                loan.Status = LoanStatus.Paid;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Applied payment {Amount} to loan {LoanId}; new balance {Balance}, status {Status}",
                request.Amount, loan.Id, loan.CurrentBalance, loan.Status);

            return LoanResponse.FromEntity(loan);
        }
    }
}
