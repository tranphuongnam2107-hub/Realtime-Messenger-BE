using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models.DTO;
using Models.DTO.Request;
using Models.DTO.Response;
using Models.Model;
using Repositories.Implement;
using Repositories.Interface;
using Services.Interface;

namespace Services.Implement
{
    public class ChatService : IChatService
    {
        private readonly IChatRepository _chatRepository;
        private readonly IMessageRepository _messageRepository;
        private readonly IUploadService _uploadService;
        private readonly IUserContextService _userContextService;

        public ChatService(IChatRepository chatRepository, IUploadService uploadService, IUserContextService userContextService, IMessageRepository messageRepository)
        {
            _chatRepository = chatRepository;
            _uploadService = uploadService;
            _userContextService = userContextService;
            _messageRepository = messageRepository;
        }

        public async Task<BaseResponseDTO<ChatResponseDTO>> CreateNewChat(CreateChatRequestDTO? request)
        {
            if (request == null)
                return BaseResponseDTO<ChatResponseDTO>.Fail("Request cannot be null.", null, null, 400);

            var loggedUser = _userContextService.GetAccountIdFromToken();
            if (string.IsNullOrEmpty(loggedUser))
                return BaseResponseDTO<ChatResponseDTO>.Fail("Unauthorized: Missing user context.", null, null, 401);

            //1. Thêm mới đoạn chat
            var newGroupAvatar = await _uploadService.UploadAsync(request.GroupAvatar, "Chat");

            var chatRequest = new Chat
            {
                ChatId = null,
                TypeChat = request.ChatType,
                LastMessage = null,
                LastMessageAt = null,
                CreatedAt = DateTime.UtcNow,
                GroupAvatar = newGroupAvatar.FileUrl,
                GroupName = request.GroupName,
                CreatedBy = loggedUser,
                IsDeleted = false
            };

            var newChat = await _chatRepository.CreateChatAsync(chatRequest);
            if (newChat == null)
                return BaseResponseDTO<ChatResponseDTO>.Fail("Cannot create new chat", null, null, 500);


            //2. Thêm mới thành viên cho đoạn chat đó
            var chatMembers = new List<ChatMember>();

            
            chatMembers.Add(new ChatMember
            {
                MemberId = null,
                ChatId = newChat.ChatId,
                AccountId = loggedUser,
                RoleDescription = (request.ChatType == ChatType.Private) ? ChatRole.Member : ChatRole.AdminGroup,
                JoinedAt = DateTime.UtcNow
            });

            if (request.ChatMembers != null && request.ChatMembers.Any())
            {
                foreach (var member in request.ChatMembers)
                {
                    if (member.AccountId == loggedUser)
                        continue;

                    chatMembers.Add(new ChatMember
                    {
                        MemberId = null,
                        ChatId = newChat.ChatId,
                        AccountId = member.AccountId,
                        RoleDescription = member.RoleDescription,
                        JoinedAt = DateTime.UtcNow
                    });
                }
            }

            var addTasks = chatMembers.Select(m => _chatRepository.AddNewMember(m));
            var insertedMembers = await Task.WhenAll(addTasks);

            if (insertedMembers.Any(x => x == null))
                return BaseResponseDTO<ChatResponseDTO>.Fail("Failed to add some members.", null, null, 500);

            var result = new ChatResponseDTO
            {
                ChatInformation = newChat,
                ChatMembers = insertedMembers.ToList()
            };

            return BaseResponseDTO<ChatResponseDTO>.Success("Create new chat success.", result, 200);
        }

        public async Task<BaseResponseDTO<List<ListChatItemDTOResponse>>> ListChatOfUser()
        {
            //1. Get user id base on the token in header
            var loggedUserId = _userContextService.GetAccountIdFromToken();

            if (loggedUserId == null)
                return BaseResponseDTO<List<ListChatItemDTOResponse>>.Fail("Unauthorized: Missing user context.", null, null, 500);

            //2. Get list chat of user id
            var chats = await _chatRepository.GetChatsOfUser(loggedUserId);

            if (chats == null || !chats.Any())
                return BaseResponseDTO<List<ListChatItemDTOResponse>>.Fail("List chat of user is null or empty", null, null, 404);

            var result = new List<ListChatItemDTOResponse>();

            foreach (var chat in chats)
            {
                var member = await _chatRepository.GetByChatAndUserIdAsync(chat.ChatId, loggedUserId);
                if (member == null) continue;

                // Nếu UnreadCount = 0 thì có thể cập nhật theo LastReadAt
                if (member.UnreadCountMessage == 0)
                {
                    var unreadCount = (int)await _messageRepository.CountMessagesAfter(chat.ChatId, member.LastReadAt);
                    await _chatRepository.UpdateUnreadCount(chat.ChatId, loggedUserId, unreadCount);
                    member.UnreadCountMessage = unreadCount;
                }

                result.Add(new ListChatItemDTOResponse
                {
                    ChatId = chat.ChatId,
                    TypeChat = chat.TypeChat,
                    LastMessage = chat.LastMessage,
                    LastMessageAt = chat.LastMessageAt,
                    UnreadCountMessage = member.UnreadCountMessage,
                    GroupName = chat.GroupName,
                    GroupAvatar = chat.GroupAvatar,
                    CreatedAt = chat.CreatedAt,
                    CreatedBy = chat.CreatedBy,
                    IsDeleted = chat.IsDeleted
                });
            }

            return BaseResponseDTO<List<ListChatItemDTOResponse>>.Success("Get list chat of user successfully", result, 200);
        }

        
    }
}
