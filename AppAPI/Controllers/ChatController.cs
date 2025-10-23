using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models.DTO.Request;
using Services.Interface;

namespace AppAPI.Controllers
{
    [Route("api/chat")]
    [ApiController]
    [Authorize]
    public class ChatController : ControllerBase
    {
        
        private readonly IChatService _chatService;
        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateNewChat([FromForm] CreateChatRequestDTO request)
        {
            if (request == null)
                return BadRequest("Request is invalid.");

            var result = await _chatService.CreateNewChat(request);

            return StatusCode(result.Status, result);
        }

        [HttpGet("chats-of-user")]
        public async Task<IActionResult> GetChatsOfUser()
        {
            var result = await _chatService.ListChatOfUser();

            return StatusCode(result.Status, result);
        }
    }
}
