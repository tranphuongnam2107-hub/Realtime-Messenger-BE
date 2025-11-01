using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Models.DTO;
using Models.DTO.Request;
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

        public async Task<BaseResponseDTO<FriendRequestsListDTO>> GetAllFriendRequest()
        {
            var loggedUserId = _userContextService.GetAccountIdFromToken();

            if (loggedUserId == null)
                return BaseResponseDTO<FriendRequestsListDTO>.Fail("Unauthorized: Missing user context.", null, null, 500);

            var incomingFriendRequest = await _friendRepository.GetIncomingFriendRequests(loggedUserId);
            var outgoingFriendRequest = await _friendRepository.GetOutgoingFriendRequests(loggedUserId);

            var allUserIds = incomingFriendRequest
                .Concat(outgoingFriendRequest)
                .SelectMany(f => new[] { f.SenderId, f.ReceiverId })
                .Distinct()
                .ToList();

            var accounts = await _accountRepository.GetAccountsByIdentifier(allUserIds);
            var accountDict = accounts.ToDictionary(a => a.AccountId, a => a);

            FriendResponseDTO MapToFriendResponseDTO(Friend friend)
            {
                accountDict.TryGetValue(friend.SenderId, out var sender);
                accountDict.TryGetValue(friend.ReceiverId, out var receiver);

                return new FriendResponseDTO
                {
                    FriendId = friend.FriendId,
                    StatusFriend = friend.StatusFriend,
                    CreatedAt = friend.CreatedAt,
                    UpdatedAt = friend.UpdatedAt,
                    SenderInfor = sender != null ? _mapper.Map<ProfileResponseDTO>(sender) : null,
                    ReceiverInfor = receiver != null ? _mapper.Map<ProfileResponseDTO>(receiver) : null
                };
            }

            var incomingDtos = incomingFriendRequest.Select(MapToFriendResponseDTO).ToList();
            var outgoingDtos = outgoingFriendRequest.Select(MapToFriendResponseDTO).ToList();

            var result = new FriendRequestsListDTO
            {
                IncomingRequest = incomingDtos,
                OutgoingRequest = outgoingDtos
            };

            return BaseResponseDTO<FriendRequestsListDTO>.Success("OK", result, 200);
        }

        public async Task<BaseResponseDTO<List<FriendResponseDTO>>> GetAlreadyFriends()
        {
            var loggedUserId = _userContextService.GetAccountIdFromToken();

            if (loggedUserId == null)
                return BaseResponseDTO<List<FriendResponseDTO>>.Fail("Unauthorized: Missing user context.", null, null, 500);

            var loggedUser = await _accountRepository.GetAccountByAccountId(loggedUserId);
            var loggedUserInfor = _mapper.Map<ProfileResponseDTO>(loggedUser);

            return null;
        }

        public async Task<BaseResponseDTO<FriendResponseDTO>> ResponseFriendRequest(ResponseFriendRequestDTO request)
        {
            var friend = await _friendRepository.GetFriendByFriendId(request.FriendId);

            if (friend == null)
                return BaseResponseDTO<FriendResponseDTO>.Fail("Cannot find this friend.", null, null, 404);

            //Lấy thông tin của 2 user
            var loggedUserId = _userContextService.GetAccountIdFromToken();
            var senderUserId = friend.SenderId;

            var loggedUser = await _accountRepository.GetAccountByAccountId(loggedUserId);
            var senderUser = await _accountRepository.GetAccountByAccountId(senderUserId);

            if (loggedUser == null || senderUser == null)
                return BaseResponseDTO<FriendResponseDTO>.Fail("Cannot find this user.", null, null, 404);

            //Mapping dữ liệu của 2 user
            var loggedUserInfor = _mapper.Map<ProfileResponseDTO>(loggedUser);
            var senderUserInfor = _mapper.Map<ProfileResponseDTO>(senderUser);

            //Update trạng thái friend thành Accepted
            var updatedStatus = await _friendRepository.UpdateStatusFriend(friend.FriendId, request.NewStatus);
            if(updatedStatus == null)
                return BaseResponseDTO<FriendResponseDTO>.Fail("Fail to response friend request.", null, null, 500);

            var result = new FriendResponseDTO
            {
                FriendId = friend.FriendId,
                SenderInfor = senderUserInfor,
                ReceiverInfor = loggedUserInfor,
                UpdatedAt = DateTime.UtcNow,
                StatusFriend = updatedStatus.StatusFriend
            };

            //Gửi thông báo lại cho người gửi lời mời
            await _hubContext.Clients.User(senderUserInfor.AccountId)
                .SendAsync("ResponseFriendRequest", result);

            return BaseResponseDTO<FriendResponseDTO>.Success("Response friend request successfully.", result, 200);
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
