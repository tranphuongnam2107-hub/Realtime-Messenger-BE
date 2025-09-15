using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess.Context;
using Models.Model;
using MongoDB.Driver;

namespace DataAccess.DAOs
{
    public class MessageDAO : BaseDAO<Message>
    {
        public MessageDAO(MongoDBContext context) : base(context, "Message")
        {
        }

        public async Task CreateMessageAsync(Message message)
        {
            await _collection.InsertOneAsync(message);
        }

        public async Task<List<Message>> GetMessagesByChatIdAsync(string chatId, int pageSize = 5, string? lastMessageId = null)
        {
            var filter = Builders<Message>.Filter.Eq(m => m.ChatId, chatId);

            if (!string.IsNullOrEmpty(lastMessageId))
            {
                var lastMessage = await _collection.Find(m => m.MessageId == lastMessageId).FirstOrDefaultAsync();
                if (lastMessage != null)
                {
                    filter = Builders<Message>.Filter.And(filter,
                        Builders<Message>.Filter.Lt(m => m.CreatedAt, lastMessage.CreatedAt));
                }
            }

            return await _collection
                .Find(filter)
                .SortByDescending(m => m.CreatedAt)
                .Limit(pageSize)
                .ToListAsync();
        }
    }
}
