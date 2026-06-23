namespace Fundo.Applications.WebApi.Models
{
    public class Loan
    {
        public Guid Id { get; set; }

        public decimal Amount { get; set; }

        public decimal CurrentBalance { get; set; }

        public string ApplicantName { get; set; } = string.Empty;

        public LoanStatus Status { get; set; }
    }
}
