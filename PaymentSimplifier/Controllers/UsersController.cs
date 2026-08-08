using Microsoft.AspNetCore.Mvc;
using PaymentSimplifier.Application.Services;
using PaymentSimplifier.Dtos;

namespace PaymentSimplifier.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] CreateUserRequest request)
        {
            try
            {
                var response = await _userService.CreateUserAsync(request);
                return Created($"/Users/{response.Id}", response);
            }
            catch (Exception ex)
            {
                return BadRequest($"An error occurred: {ex.Message}");
            }
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
