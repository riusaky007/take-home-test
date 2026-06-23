namespace Fundo.Applications.WebApi.Services
{
    public class LoanNotFoundException : Exception
    {
        public LoanNotFoundException(Guid id)
            : base($"Loan with id '{id}' was not found.")
        {
        }
    }

    public class InvalidPaymentException : Exception
    {
        public InvalidPaymentException(string message) : base(message)
        {
        }
    }
}
