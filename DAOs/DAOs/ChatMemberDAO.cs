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
    public class ChatMemberDAO : BaseDAO<ChatMember>
    {
        public ChatMemberDAO(MongoDBContext context) : base(context, "ChatMember")
        {
        }

        public async Task<ChatMember?> AddChatMember(ChatMember request)
        {
            try
            {
                if (request.JoinedAt == default)
                    request.JoinedAt = DateTime.UtcNow;

                await _collection.InsertOneAsync(request);
                return request;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error create ChatMember: {ex.Message}");
                return null;
            }
        }

        public async Task<ChatMember?> UpdateLastReadMessage(string memberId)
        {
            try
            {
                var filter = Builders<ChatMember>.Filter.Eq(x => x.MemberId, memberId);
                var update = Builders<ChatMember>.Update
                    .Set(x => x.LastReadAt, DateTime.UtcNow);

                var options = new FindOneAndUpdateOptions<ChatMember>
                {
                    ReturnDocument = ReturnDocument.After
                };

                var updatedMember = await _collection.FindOneAndUpdateAsync(filter, update, options);
                return updatedMember;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error update LastReadAt: {ex.Message}");
                return null;
            }
        }

        public async Task<List<ChatMember>?> GetChatMembersOfUserAsync(string accountId)
        {
            return await _collection.Find(cm => cm.AccountId == accountId).ToListAsync();
        }

        public async Task<ChatMember> GetChatMemberAsync(string chatId, string accountId)
        {
            return await _collection.Find(cm => cm.ChatId == chatId && cm.AccountId == accountId).FirstOrDefaultAsync();
        }

        public async Task UpdateUnreadCountAsync(string chatId, string accountId, int unreadCount)
        {
            var update = Builders<ChatMember>.Update.Set(cm => cm.UnreadCountMessage, unreadCount);
            await _collection.UpdateOneAsync(cm => cm.ChatId == chatId && cm.AccountId == accountId, update);
        }

        public async Task IncreaseUnreadCountExceptSenderAsync(string chatId, string senderId)
        {
            var filter = Builders<ChatMember>.Filter.And(
                Builders<ChatMember>.Filter.Eq(cm => cm.ChatId, chatId),
                Builders<ChatMember>.Filter.Ne(cm => cm.AccountId, senderId)
            );

            var update = Builders<ChatMember>.Update.Inc(cm => cm.UnreadCountMessage, 1);
            await _collection.UpdateManyAsync(filter, update);
        }

        public async Task<List<ChatMember>> GetChatMemberByChatId(string chatId)
        {
            return await _collection.Find(cm => cm.ChatId == chatId).ToListAsync();
        }
    } 
}
