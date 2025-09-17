using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Models.DTO;
using Models.DTO.Request;
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

        public MessageService(IChatRepository chatRepository, IMessageRepository messageRepository, IUploadService uploadService, IHubContext<ChatHub> hubContext)
        {
            _chatRepository = chatRepository;
            _messageRepository = messageRepository;
            _uploadService = uploadService;
            _hubContext = hubContext;
        }

        public Task<List<Message>> GetMessagesAsync(string chatId, int pageSize = 5, string? lastMessageId = null)
        {
            throw new NotImplementedException();
        }

        public async Task<BaseResponseDTO<string>> SendMessageAsync(SendMessageRequestDTO request, string accId)
        {
           try
           {
                //1. Validate input
                if (string.IsNullOrEmpty(accId))
                    return BaseResponseDTO<string>.Fail("Account is not found.", null, null, 400);

                if (string.IsNullOrEmpty(request.ChatId))
                    return BaseResponseDTO<string>.Fail("Chat Id is null or invalid", null, null, 400);

                if (string.IsNullOrEmpty(request.Type))
                    return BaseResponseDTO<string>.Fail("Type message is null or invalid", null, null, 400);

                //LẤY THÔNG TIN CHAT TỪ CHAT ID
                var chat = await _chatRepository.GetChatByIdAsync(request.ChatId);
                if (chat == null)
                    return BaseResponseDTO<string>.Fail("This chat is not found.", null, null, 404);

                var uploadedImageUrls = new List<string>();
                var uploadedFileUrls = new List<string>();


                // 2. Upload images lên Cloudinary
                if (request.Images != null && request.Images.Any())
                {
                    foreach (var image in request.Images)
                    {
                        var imageUrl = await _uploadService.UploadAsync(image, "chat/images");
                        uploadedImageUrls.Add(imageUrl);
                    }
                }

                // 3. Upload files lên Cloudinary
                if (request.Files != null && request.Files.Any())
                {
                    foreach (var file in request.Files)
                    {
                        var fileUrl = await _uploadService.UploadAsync(file, "chat/files");
                        uploadedFileUrls.Add(fileUrl);
                    }
                }

                // 4. Tạo Message object
                var newMessage = new Message
                {
                    ChatId = request.ChatId,
                    SenderId = accId,
                    Type = request.Type,
                    TextMessage = request.TextMessage,
                    Images = uploadedImageUrls.Any() ? uploadedImageUrls : null,
                    Files = uploadedFileUrls.Any() ? uploadedFileUrls : null,
                    CreatedAt = DateTime.UtcNow,
                    Status = "sent"
                };

                await _messageRepository.CreateMessageAsync(newMessage);

                // 5. Update LastMessage trong Chat
                chat.LastMessage = !string.IsNullOrEmpty(request.TextMessage) ? request.TextMessage :
                                   uploadedImageUrls.Any() ? "[Image]" :
                                   uploadedFileUrls.Any() ? "[File]" : "[Message]";

                await _chatRepository.UpdateLastMessageAsync(chat.ChatId, chat.LastMessage); //lastMessageAt đã có setup trong dao rồi

                // 6. Gửi real-time notification qua SignalR
                await _hubContext.Clients.Group(request.ChatId).SendAsync("ReceiveMessage", new
                {
                    ChatId = request.ChatId,
                    MessageId = newMessage.MessageId,
                    SenderId = newMessage.SenderId,
                    Type = newMessage.Type,
                    TextMessage = newMessage.TextMessage,
                    Images = newMessage.Images,
                    Files = newMessage.Files,
                    CreatedAt = newMessage.CreatedAt,
                    Status = newMessage.Status
                });

                return BaseResponseDTO<string>.Success("Send message success.", newMessage.ChatId, 200);

           } catch (Exception ex)
           {
               return BaseResponseDTO<string>.Fail("Send message fail.", null, new List<string> { ex.Message }, 500);
           }
            
        }
    }
}
