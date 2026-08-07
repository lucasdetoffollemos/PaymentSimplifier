using Microsoft.AspNetCore.Mvc;
using PaymentSimplifier.Application.Services;

namespace PaymentSimplifier.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TransfersController : ControllerBase
    {
        private readonly IUserService _userService;
        public TransfersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPatch("{userId}/deposit")]
        public async Task<IActionResult> DepositAsync(Guid userId, [FromBody] decimal amount)
        {
            try
            {
                var response = await _userService.DepositInUserAccountAsync(userId, amount);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest($"An error occurred: {ex.Message}");
            }
                
            
        }
    }
}
