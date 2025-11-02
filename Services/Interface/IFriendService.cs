using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models.DTO;
using Models.DTO.Request;
using Models.DTO.Response;
using Models.Model;

namespace Services.Interface
{
    public interface IFriendService
    {
        Task<BaseResponseDTO<FriendResponseDTO>> SendFriendRequest(string? toUserId);
        Task<BaseResponseDTO<FriendResponseDTO>> ResponseFriendRequest(ResponseFriendRequestDTO request);
        Task<BaseResponseDTO<FriendRequestsListDTO>> GetAllFriendRequest();
        Task<BaseResponseDTO<List<FriendListItemDTO>>> GetAlreadyFriends();
    }
}
