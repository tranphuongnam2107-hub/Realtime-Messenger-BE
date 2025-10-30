using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Models.DTO;
using Models.DTO.Response;
using Models.Model;
using Repositories.Interface;
using Services.Hubs;
using Services.Interface;

namespace Services.Implement
{
    public class FriendService : IFriendService
    {
        private readonly IFriendRepository _friendRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly IUserContextService _userContextService;
        private readonly IMapper _mapper;
        private readonly IHubContext<ChatHub> _hubContext;

        public FriendService(IFriendRepository friendRepository, IAccountRepository accountRepository, IUserContextService userContextService, IMapper mapper, IHubContext<ChatHub> hubContext)
        {
            _friendRepository = friendRepository;
            _accountRepository = accountRepository;
            _userContextService = userContextService;
            _mapper = mapper;
            _hubContext = hubContext;
        }

        public async Task<BaseResponseDTO<FriendResponseDTO>> SendFriendRequest(string? toUserId)
        {
            if (string.IsNullOrEmpty(toUserId))
                return BaseResponseDTO<FriendResponseDTO>.Fail("Receiver ID is invalid.", null, null, 400);

            var loggedUserId = _userContextService.GetAccountIdFromToken();

            if(loggedUserId == toUserId) 
                return BaseResponseDTO<FriendResponseDTO>.Fail("You cannot send a friend request to yourself.", null, null, 400);

            var loggedUser = await _accountRepository.GetAccountByAccountId(loggedUserId);
            var toUser = await _accountRepository.GetAccountByAccountId(toUserId);

            if (loggedUser == null || toUser == null)
                return BaseResponseDTO<FriendResponseDTO>.Fail("Cannot find this user.", null, null, 404);

            //Mapping dữ liệu của 2 user
            var loggedUserInfor = _mapper.Map<ProfileResponseDTO>(loggedUser);
            var toUserInfor = _mapper.Map<ProfileResponseDTO>(toUser);

            //Kiểm tra 2 user đã là bạn hay chưa
            var existingFriend = await _friendRepository.GetFriendByUsers(loggedUser.AccountId, toUser.AccountId);
            if (existingFriend != null)
            {
                return BaseResponseDTO<FriendResponseDTO>.Success($"Friend request already exists with status '{existingFriend.StatusFriend}'.", null, 400);
            }

            //Tạo object Friend trạng thái Requested
            var friendRequest = new Friend
            {
                SenderId = loggedUser.AccountId,
                ReceiverId = toUser.AccountId,
                StatusFriend = StatusFriend.Requested,
                CreatedAt = DateTime.UtcNow,
            };

            var newFriend = await _friendRepository.AddNewFriend(friendRequest);
            if(newFriend == null)
                return BaseResponseDTO<FriendResponseDTO>.Fail("Fail to add friend.", null, null, 500);

            var result = _mapper.Map<FriendResponseDTO>(newFriend);
            result.SenderInfor = loggedUserInfor;
            result.ReceiverInfor = toUserInfor;

            //Gửi cho người nhận
            await _hubContext.Clients.User(toUser.AccountId)
                .SendAsync("ReceiveFriendRequest", result);

            //Dùng để chuyển đổi nút send
            await _hubContext.Clients.User(loggedUser.AccountId)
                .SendAsync("FriendRequestSent", result);

            return BaseResponseDTO<FriendResponseDTO>.Success("Add friend successfully.", result, 200);
        }
    }
}
