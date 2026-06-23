using Fundo.Applications.WebApi.Models;

namespace Fundo.Applications.WebApi.Dtos
{
    public class LoanResponse
    {
        public Guid Id { get; set; }

        public decimal Amount { get; set; }

        public decimal CurrentBalance { get; set; }

        public string ApplicantName { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public static LoanResponse FromEntity(Loan loan) => new()
        {
            Id = loan.Id,
            Amount = loan.Amount,
            CurrentBalance = loan.CurrentBalance,
            ApplicantName = loan.ApplicantName,
            Status = loan.Status.ToString().ToLowerInvariant()
        };
    }
}
