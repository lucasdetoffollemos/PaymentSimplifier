using Microsoft.AspNetCore.Mvc;
using PaymentSimplifier.Application.Services;
using PaymentSimplifier.Dtos;

namespace PaymentSimplifier.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TransfersController : ControllerBase
    {
        private readonly ITransferService _transferService;
        public TransfersController(ITransferService transferService)
        {
            _transferService = transferService;
        }

        [HttpPost]
        public async Task<IActionResult> TransferAsync([FromBody] TransferRequest request)
        {
            try
            {
                var response = await _transferService.TransferAsync(request.PayerId, request.PayeeId, request.Value);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest($"An error occurred while transferring: {ex.Message}");
            }
        }
    }
}
