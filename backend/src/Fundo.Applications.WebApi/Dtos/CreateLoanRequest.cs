using System.ComponentModel.DataAnnotations;

namespace Fundo.Applications.WebApi.Dtos
{
    public class CreateLoanRequest
    {
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string ApplicantName { get; set; } = string.Empty;
    }
}
