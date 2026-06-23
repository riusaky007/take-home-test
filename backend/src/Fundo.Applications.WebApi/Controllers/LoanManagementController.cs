using Fundo.Applications.WebApi.Dtos;
using Fundo.Applications.WebApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace Fundo.Applications.WebApi.Controllers
{
    [ApiController]
    [Route("loans")]
    [Produces("application/json")]
    public class LoanManagementController : ControllerBase
    {
        private readonly ILoanService _loanService;

        public LoanManagementController(ILoanService loanService)
        {
            _loanService = loanService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<LoanResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<LoanResponse>>> GetAll(CancellationToken cancellationToken)
        {
            var loans = await _loanService.GetAllAsync(cancellationToken);
            return Ok(loans);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(LoanResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<LoanResponse>> GetById(Guid id, CancellationToken cancellationToken)
        {
            var loan = await _loanService.GetByIdAsync(id, cancellationToken);
            return loan is null ? NotFound() : Ok(loan);
        }

        [HttpPost]
        [ProducesResponseType(typeof(LoanResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<LoanResponse>> Create(
            [FromBody] CreateLoanRequest request,
            CancellationToken cancellationToken)
        {
            var loan = await _loanService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = loan.Id }, loan);
        }

        [HttpPost("{id:guid}/payment")]
        [ProducesResponseType(typeof(LoanResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<LoanResponse>> ApplyPayment(
            Guid id,
            [FromBody] PaymentRequest request,
            CancellationToken cancellationToken)
        {
            var loan = await _loanService.ApplyPaymentAsync(id, request, cancellationToken);
            return Ok(loan);
        }
    }
}
