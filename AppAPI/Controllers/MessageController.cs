using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models.DTO.Request;
using Services.Interface;

namespace AppAPI.Controllers
{
    [Route("api/message")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        private readonly IMessageService _messageService;

        public MessageController(IMessageService messageService)
        {
            _messageService = messageService;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromForm] SendMessageRequestDTO request)
        {
            if (request == null) return BadRequest("Request is null.");

            var result = await _messageService.SendMessageAsync(request); 

            return StatusCode(result.Status, result);
        }

        [HttpGet("messages/{chatId}")]
        public async Task<IActionResult> GetMessagesOfChat([FromRoute] string? chatId,
                                                            [FromQuery] int pageSize = 5,
                                                            [FromQuery] string? lastMessageId = null)
        {
            if (string.IsNullOrEmpty(chatId))
                return BadRequest("Chat ID is not valid.");

            var result = await _messageService.GetMessagesAsync(chatId, pageSize, lastMessageId);

            return StatusCode(result.Status, result);
        }
    }
}
