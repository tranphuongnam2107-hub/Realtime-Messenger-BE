using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models.DTO;
using Models.DTO.Request;
using Models.DTO.Response;
using Services.Interface;

namespace AppAPI.Controllers
{
    [Route("api/authen")]
    [ApiController]
    public class AuthenController : ControllerBase
    {
        private readonly IAuthenticationService _authenService;

        public AuthenController(IAuthenticationService authenService)
        {
            _authenService = authenService;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginRequestDTO request)
        {
            var result = await _authenService.Login(request);

            if (result == null)
                return BadRequest("Request is invalid.");

            if (result.Status != 200)
                return StatusCode(result.Status, result);

            return Ok(result);
        }

        
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var result = await _authenService.Logout();
            if (result == null)
                return NotFound();

            return StatusCode(result.Status, result);
        }

        [AllowAnonymous]
        [HttpPost("refresh-token")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDTO request)
        {
            if (request == null)
                return BadRequest("Request is invalid.");

            var result = await _authenService.ValidateRefreshToken(request.RefreshToken);

            return StatusCode(result.Status, result);
        }
    }
}
