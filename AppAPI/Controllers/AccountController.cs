using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Interface;

namespace AppAPI.Controllers
{
    [Route("api/account")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpGet("searching")]
        public async Task<IActionResult> SearchUser([FromQuery] string? identify)
        {
            if (identify == null)
                return BadRequest("Identify is invalid or null.");

            var result = await _accountService.SearchUser(identify);
            return StatusCode(result.Status, result);
        }
    }
}
