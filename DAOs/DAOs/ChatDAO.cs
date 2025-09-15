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
    public class ChatDAO : BaseDAO<Chat>
    {
        public ChatDAO(MongoDBContext context) : base(context, "Chat")
        {
        }

        public async Task<Chat> GetChatByIdAsync(string chatId)
        {
            return await _collection.Find(c => c.ChatId == chatId).FirstOrDefaultAsync();
        }

        public async Task CreateChatAsync(Chat chat)
        {
            await _collection.InsertOneAsync(chat);
        }

        public async Task UpdateLastMessageAsync(string chatId, string lastMessage)
        {
            var update = Builders<Chat>.Update
                .Set(c => c.LastMessage, lastMessage)
                .Set(c => c.LastMessageAt, DateTime.UtcNow);
            await _collection.UpdateOneAsync(c => c.ChatId == chatId, update);
        }
    }
}
