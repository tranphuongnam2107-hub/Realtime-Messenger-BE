using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess.DAOs;
using Models.Model;
using Repositories.Interface;

namespace Repositories.Implement
{
    public class MessageRepository : IMessageRepository
    {
        private readonly MessageDAO _messageDao;

        public MessageRepository(MessageDAO messageDao)
        {
            _messageDao = messageDao;
        }

        public async Task CreateMessageAsync(Message message)
        {
            await _messageDao.CreateMessageAsync(message);
        }

        public async Task<Message?> GetMessageByIdAsync(string? messageId)
        {
            return await _messageDao.GetMessageByIdsAsync(messageId);
        }

        public async Task<List<Message>> GetMessagesByChatIdAsync(string chatId, int pageSize = 5, string? lastMessageId = null)
        {
            return await _messageDao.GetMessagesByChatIdAsync(chatId, pageSize, lastMessageId);
        }

        public async Task<List<Message>?> GetMessagesByIdAsync(List<string>? messageIds)
        {
            return await _messageDao.GetMessagesByIdsAsync(messageIds);
        }
    }
}
