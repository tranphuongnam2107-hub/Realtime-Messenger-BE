using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models.DTO.Request;
using Services.Interface;

namespace AppAPI.Controllers
{
    [Route("api/chat")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IMessageService _messageService;
        public ChatController(IMessageService messageService)
        {
            _messageService = messageService;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromForm] SendMessageRequestDTO request)
        {
            if (request == null) return BadRequest("Request is null.");

            var result = await _messageService.SendMessageAsync(request, "68c5771cec934cc80b9b1efa");

            return StatusCode(result.Status, result);
        }
    }
}
