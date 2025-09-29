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
    public interface IMessageService
    {
        Task<BaseResponseDTO<MessageResponseDTO>> SendMessageAsync(SendMessageRequestDTO request, string accId);
        Task<List<Message>> GetMessagesAsync(string chatId, int pageSize = 5, string? lastMessageId = null);
    }
}
