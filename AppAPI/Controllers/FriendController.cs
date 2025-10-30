using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models.DTO.Request;
using Services.Interface;

namespace AppAPI.Controllers
{
    [Route("api/friend")]
    [ApiController]
    public class FriendController : ControllerBase
    {
        private readonly IFriendService _friendService;

        public FriendController(IFriendService friendService)
        {
            _friendService = friendService;
        }

        [HttpPost]
        public async Task<IActionResult> AddNewFriend([FromBody] FriendRequestDTO request)
        {
            if (request == null) return BadRequest("Request is null.");

            var result = await _friendService.SendFriendRequest(request.ToUserId);

            return StatusCode(result.Status, result);
        }
    }
}
