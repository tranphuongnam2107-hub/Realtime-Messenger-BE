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
    public class FriendDAO : BaseDAO<Friend>
    {
        public FriendDAO(MongoDBContext context) : base(context, "Friend")
        {
        }

        public async Task<List<string>> GetFriendIdsAsync(string? userId)
        {
            if(string.IsNullOrEmpty(userId))
                return new List<string>();

            var filter = Builders<Friend>.Filter.And(
                Builders<Friend>.Filter.Eq(f => f.StatusFriend, StatusFriend.Accepted),
                Builders<Friend>.Filter.Or(
                    Builders<Friend>.Filter.Eq(f => f.SenderId, userId),
                    Builders<Friend>.Filter.Eq(f => f.ReceiverId, userId)
                )
            );

            var friends = await _collection.Find(filter).ToListAsync();

            var friendIds = friends.Select(f =>
                f.SenderId == userId ? f.ReceiverId : f.SenderId
            ).Distinct().ToList();

            return friendIds;
        }

        public async Task<Friend?> AddFriend(Friend? friend)
        {
            if (friend == null)
                return null;

            await _collection.InsertOneAsync(friend);
            return friend;
        }

        public async Task<Friend?> GetFriendByUsers(string? firstUser, string? secondUser)
        {
            if (string.IsNullOrEmpty(firstUser) || string.IsNullOrEmpty(secondUser))
                return null;

            var filter = Builders<Friend>.Filter.Or(
                Builders<Friend>.Filter.And(
                    Builders<Friend>.Filter.Eq(f => f.SenderId, firstUser),
                    Builders<Friend>.Filter.Eq(f => f.ReceiverId, secondUser)
                ),
                Builders<Friend>.Filter.And(
                    Builders<Friend>.Filter.Eq(f => f.SenderId, secondUser),
                    Builders<Friend>.Filter.Eq(f => f.ReceiverId, firstUser)
                )
            );

            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<Friend?> UpdateStatusFriend(string? friendId, StatusFriend newStatus)
        {
            if (string.IsNullOrEmpty(friendId))
                return null;

            var filter = Builders<Friend>.Filter.Eq(f => f.FriendId, friendId);
            var update = Builders<Friend>.Update.Set(f => f.StatusFriend, newStatus)
                .Set(f => f.UpdatedAt, DateTime.UtcNow);

            return await _collection.FindOneAndUpdateAsync(
                filter,
                update,
                new FindOneAndUpdateOptions<Friend>
                {
                    ReturnDocument = ReturnDocument.After
                });
        }

        public async Task<Friend?> GetFriendById(string? friendId)
        {
            if (string.IsNullOrEmpty(friendId))
                return null;

            var filter = Builders<Friend>.Filter.Eq(f => f.FriendId, friendId);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<List<Friend>> GetIncomingFriendRequests(string? accountId)
        {
            var filter = Builders<Friend>.Filter.And(
                Builders<Friend>.Filter.Eq(f => f.ReceiverId, accountId),
                Builders<Friend>.Filter.Eq(f => f.StatusFriend, StatusFriend.Requested)
            );

            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<List<Friend>> GetOutgoingFriendRequests(string accountId)
        {
            var filter = Builders<Friend>.Filter.And(
                Builders<Friend>.Filter.Eq(f => f.SenderId, accountId),
                Builders<Friend>.Filter.Eq(f => f.StatusFriend, StatusFriend.Requested)
            );
            return await _collection.Find(filter).ToListAsync();
        }
    }
}
