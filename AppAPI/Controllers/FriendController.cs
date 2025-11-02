using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models.DTO.Request;
using Services.Interface;

namespace AppAPI.Controllers
{
    [Route("api/friends")]
    [ApiController]
    [Authorize]
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

        [HttpPut]
        public async Task<IActionResult> ResponseFriendRequest([FromBody] ResponseFriendRequestDTO request)
        {
            if (request == null) return BadRequest("Request is invalid.");

            var result = await _friendService.ResponseFriendRequest(request);

            return StatusCode(result.Status, result);
        }

        [HttpGet("requests")]
        public async Task<IActionResult> GetAllFriendRequest()
        {
            var result = await _friendService.GetAllFriendRequest();

            return StatusCode(result.Status, result);
        }

        [HttpGet] 
        public async Task<IActionResult> GetAllFriend()
        {
            var result = await _friendService.GetAlreadyFriends();
            return StatusCode(result.Status, result);
        }
    }
}
