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
                Builders<Friend>.Filter.Eq(f => f.StatusFriend, "Accepted"),
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
    }
}
