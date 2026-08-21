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
                var (canTransfer, canNotify) = await _transferService.TransferAsync(request.PayerId, request.PayeeId, request.Value, request.Password);


                if(!canTransfer)
                {
                    return BadRequest("Transfer cannot be completed because authorization not granted.");
                }   

                if(!canNotify)
                {
                    return Ok("Transfer completed but notification could not be sent.");
                }

                return Ok("Transfer completed and notification sent successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest($"An error occurred while transferring: {ex.Message}");
            }
        }
    }
}
