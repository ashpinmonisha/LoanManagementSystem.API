using LoanManagementSystem.Application.ServiceInterface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoanManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/emis")]
    [Authorize(Roles = "Admin,Manager")]
    public class EmisController : ControllerBase
    {
        private readonly ILoanOutstandingDataReader _reader;
        private readonly IOutstandingBalanceService _outstandingBalanceService;

        public EmisController(
            ILoanOutstandingDataReader reader,
            IOutstandingBalanceService outstandingBalanceService)
        {
            _reader = reader;
            _outstandingBalanceService = outstandingBalanceService;
        }

        // GET: api/emis/loan/5
        [HttpGet("loan/{loanId:int}")]
        public async Task<IActionResult> GetLoanEmis(int loanId)
        {
            if (loanId <= 0)
            {
                return BadRequest(new
                {
                    message = "Invalid Loan ID"
                });
            }

            var emis = await _reader.GetEmisAsync(loanId);

            if (emis.Count == 0)
            {
                return Ok(new
                {
                    loanId,
                    message = "EMI schedule not initialized",
                    installments = emis
                });
            }

            return Ok(emis);
        }

        // GET: api/emis/outstanding/5
        [HttpGet("outstanding/{loanId:int}")]
        public async Task<IActionResult> GetOutstanding(int loanId)
        {
            if (loanId <= 0)
            {
                return BadRequest(new
                {
                    message = "Invalid Loan ID"
                });
            }

            var outstanding = await _outstandingBalanceService
                .GetOutstandingBalanceAsync(loanId);

            return Ok(outstanding);
        }
    }
}