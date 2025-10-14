using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
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
    public class MessageService : IMessageService
    {
        private readonly IChatRepository _chatRepository;
        private readonly IMessageRepository _messageRepository;
        private readonly IUploadService _uploadService;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly IAccountRepository _accountRepository;
        private readonly IMapper _mapper;
        private readonly IUserContextService _userContextService;

        public MessageService(IChatRepository chatRepository, IMessageRepository messageRepository, IUploadService uploadService, IHubContext<ChatHub> hubContext, IAccountRepository accountRepository, IMapper mapper, IUserContextService userContextService)
        {
            _chatRepository = chatRepository;
            _messageRepository = messageRepository;
            _uploadService = uploadService;
            _hubContext = hubContext;
            _accountRepository = accountRepository;
            _mapper = mapper;
            _userContextService = userContextService;
        }

        public async Task<BaseResponseDTO<List<MessageResponseDTO>>> GetMessagesAsync(string chatId, int pageSize = 5, string? lastMessageId = null)
        {
            //Lấy accountid người dùng
            var accountId = _userContextService.GetAccountIdFromToken();

            //1. Lấy danh sách message từ chat Id
            var chat = await _chatRepository.GetChatByIdAsync(chatId);
            if (chat == null)
                return BaseResponseDTO<List<MessageResponseDTO>>.Fail("Cannot get this chat.", null, null, 404);

            var messages = await _messageRepository.GetMessagesByChatIdAsync(chat.ChatId, pageSize, lastMessageId) ?? new List<Message>();
            if (!messages.Any())
                return BaseResponseDTO<List<MessageResponseDTO>>.Fail("No messages of this chat.", null, null, 204);

            //2. Lấy danh sách id của sender và message id
            var senderIds = messages.Select(mess => mess.SenderId).Distinct().ToList();
            var replyToIds = messages.Where(mess => !string.IsNullOrEmpty(mess.ReplyToMessageId))
                                                    .Select(mess => mess.ReplyToMessageId!).Distinct().ToList();

            //3. Query tất cả senders và replies 1 lần
            var senders = await _accountRepository.GetAccountsByIdentifier(senderIds);
            var replies = await _messageRepository.GetMessagesByIdAsync(replyToIds);

            //4. Convert sang dictionary
            var senderDict = senders.ToDictionary(s => s.AccountId, s => s);
            var replyDict = replies.ToDictionary(r => r.MessageId, r => r);

            //5. Mapping dữ liệu của từng message thành MessageResponseDTO
            var messagesResponseDto = messages.Select(message =>
            {
                var messageDto = _mapper.Map<MessageResponseDTO>(message, opt => opt.Items["CurrentUserId"] = accountId);

                // Sender info
                if (senderDict.TryGetValue(message.SenderId, out var sender))
                {
                    messageDto.SenderInfor = _mapper.Map<ProfileResponseDTO>(sender);
                }

                // Reply info
                if (!string.IsNullOrEmpty(message.ReplyToMessageId)
                    && replyDict.TryGetValue(message.ReplyToMessageId!, out var replyMsg))
                {
                    var replySenderName = senderDict.TryGetValue(replyMsg.SenderId, out var replySender)
                        ? replySender.FullName
                        : "Unknown";

                    messageDto.ReplyToMessage = new ReplyMessageDTO
                    {
                        MessageId = replyMsg.MessageId,
                        SenderId = replyMsg.SenderId,
                        SenderName = replySenderName,
                        Type = replyMsg.Type,
                        TextMessage = replyMsg.TextMessage,
                        Images = replyMsg.Images,
                        Files = replyMsg.Files?.Select(f => new FileMetadata
                        {
                            FileName = f.FileName,
                            FileUrl = f.FileUrl,
                            Size = f.Size,
                            ContentType = f.ContentType
                        }).ToList()
                    };
                }

                return messageDto;
            }).ToList();

            return BaseResponseDTO<List<MessageResponseDTO>>.Success("Get messages success.", messagesResponseDto, 200);
        }

        public async Task<BaseResponseDTO<MessageResponseDTO>> SendMessageAsync(SendMessageRequestDTO request)
        {
            try
            {
                var accId = _userContextService.GetAccountIdFromToken();

                //1. Validate input
                if (string.IsNullOrEmpty(accId))
                    return BaseResponseDTO<MessageResponseDTO>.Fail("Account is not found.", null, null, 400);

                if (string.IsNullOrEmpty(request.ChatId))
                    return BaseResponseDTO<MessageResponseDTO>.Fail("Chat Id is null or invalid", null, null, 400);

                if (string.IsNullOrEmpty(request.Type))
                    return BaseResponseDTO<MessageResponseDTO>.Fail("Type message is null or invalid", null, null, 400);

                //LẤY THÔNG TIN CHAT TỪ CHAT ID
                var chat = await _chatRepository.GetChatByIdAsync(request.ChatId);
                if (chat == null)
                    return BaseResponseDTO<MessageResponseDTO>.Fail("This chat is not found.", null, null, 404);

                var uploadedImages = new List<FileMetadata>();
                var uploadedFiles = new List<FileMetadata>();


                // 2. Upload images lên Cloudinary
                if (request.Images != null && request.Images.Any())
                {
                    foreach (var image in request.Images)
                    {
                        var imageMeta = await _uploadService.UploadAsync(image, "chat/images");
                        uploadedImages.Add(imageMeta);
                    }
                }

                // 3. Upload files lên Cloudinary
                if (request.Files != null && request.Files.Any())
                {
                    foreach (var file in request.Files)
                    {
                        var fileMeta = await _uploadService.UploadAsync(file, "chat/files");
                        uploadedFiles.Add(fileMeta);
                    }
                }

                // 4. Tạo Message object
                var newMessage = new Message
                {
                    ChatId = request.ChatId,
                    SenderId = accId,
                    Type = request.Type,
                    TextMessage = request.TextMessage,
                    Images = uploadedImages.Any() ? uploadedImages : null,
                    Files = uploadedFiles.Any() ? uploadedFiles : null,
                    CreatedAt = DateTime.UtcNow,
                    Status = "sent",
                    ReplyToMessageId = request.ReplyToMessageId
                };

                await _messageRepository.CreateMessageAsync(newMessage);

                // 5. Update LastMessage trong Chat
                chat.LastMessage = !string.IsNullOrEmpty(request.TextMessage) ? request.TextMessage :
                           uploadedImages.Any() ? "[Image]" :
                           uploadedFiles.Any() ? "[File]" : "[Message]";

                await _chatRepository.UpdateLastMessageAsync(chat.ChatId, chat.LastMessage); //lastMessageAt đã có setup trong dao rồi


                //LẤY THÔNG TIN REPLY NẾU CÓ REPLY
                ReplyMessageDTO? replyMessage = null;

                if (!string.IsNullOrEmpty(request.ReplyToMessageId))
                {
                    var originalMessage = await _messageRepository.GetMessageByIdAsync(request.ReplyToMessageId);
                    if (originalMessage != null)
                    {
                        var replySender = await _accountRepository.GetAccountByIdentifier(originalMessage.SenderId);

                        replyMessage = new ReplyMessageDTO
                        {
                            MessageId = originalMessage.MessageId,
                            SenderId = originalMessage.SenderId,
                            SenderName = replySender?.FullName ?? "Unknown",
                            Type = originalMessage.Type,
                            TextMessage = originalMessage.TextMessage,
                            Images = originalMessage.Images,
                            Files = originalMessage.Files?.Select(f => new FileMetadata
                            {
                                FileName = f.FileName,
                                FileUrl = f.FileUrl,
                                Size = f.Size,
                                ContentType = f.ContentType
                            }).ToList()
                        };
                    }
                }

                //MAPPING MESSAGE RESPONSE
                var sender = await _accountRepository.GetAccountByIdentifier(accId);
                var messageResponse = new MessageResponseDTO
                {
                    MessageId = newMessage.MessageId,
                    ChatId = newMessage.ChatId,
                    SenderInfor = _mapper.Map<ProfileResponseDTO>(sender),
                    Type = newMessage.Type,
                    TextMessage = newMessage.TextMessage,
                    Images = newMessage.Images,
                    Files = newMessage.Files?.Select(f => new FileMetadata
                    {
                        FileName = f.FileName,
                        FileUrl = f.FileUrl,
                        Size = f.Size,
                        ContentType = f.ContentType
                    }).ToList(),
                    CreatedAt = newMessage.CreatedAt,
                    Status = newMessage.Status,
                    ReplyToMessage = replyMessage
                };

                // 6. Gửi real-time notification qua SignalR
                await _hubContext.Clients.Group(request.ChatId).SendAsync("ReceiveMessage", messageResponse);

                return BaseResponseDTO<MessageResponseDTO>.Success("Send message success.", messageResponse, 200);

            }
            catch (Exception ex)
            {
                return BaseResponseDTO<MessageResponseDTO>.Fail("Send message fail.", null, new List<string> { ex.Message }, 500);
            }

        }

        private async Task<ReplyMessageDTO?> GetReplyMessage(string replyToMessageId)
        {
            var originalMessage = await _messageRepository.GetMessageByIdAsync(replyToMessageId);
            if (originalMessage == null)
                return null;

            var replySender = await _accountRepository.GetAccountByIdentifier(originalMessage.SenderId);

            return new ReplyMessageDTO
            {
                MessageId = originalMessage.MessageId,
                SenderId = originalMessage.SenderId,
                SenderName = replySender?.FullName ?? "Unknown",
                Type = originalMessage.Type,
                TextMessage = originalMessage.TextMessage,
                Images = originalMessage.Images,
                Files = originalMessage.Files?.Select(f => new FileMetadata
                {
                    FileName = f.FileName,
                    FileUrl = f.FileUrl,
                    Size = f.Size,
                    ContentType = f.ContentType
                }).ToList()
            };

        }

    }
}
