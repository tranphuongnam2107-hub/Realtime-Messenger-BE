using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models.Model;

namespace Repositories.Interface
{
    public interface IMessageRepository
    {
        Task CreateMessageAsync(Message message);
        Task<List<Message>> GetMessagesByChatIdAsync(string chatId, int pageSize = 5, string? lastMessageId = null);
        Task<Message?> GetMessageByIdAsync(string? messageId);
        Task<List<Message>?> GetMessagesByIdAsync(List<string>? messageIds);
    }
}
