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
    public class AccountDAO : BaseDAO<Account>
    {
        public AccountDAO(MongoDBContext context) : base(context, "Account")
        {
            // Tạo index cho Email và Phone
            CreateIndexes().Wait();
        }

        //TẠO INDEX CHO CỘT EMAIL VÀ PHONE
        private async Task CreateIndexes()
        {
            var emailIndex = Builders<Account>.IndexKeys.Ascending(a => a.Email);
            var phoneIndex = Builders<Account>.IndexKeys.Ascending(a => a.PhoneNumber);

            await _collection.Indexes.CreateOneAsync(new CreateIndexModel<Account>(emailIndex));
            await _collection.Indexes.CreateOneAsync(new CreateIndexModel<Account>(phoneIndex));
        }

        public async Task<Account?> GetAccountByIdentify(string? identify)
        {
            if (string.IsNullOrEmpty(identify))
                return null;

            var filter = Builders<Account>.Filter.Or(
                    Builders<Account>.Filter.Eq(a => a.Email, identify),
                    Builders<Account>.Filter.Eq(a => a.PhoneNumber, identify)
                );

            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<Account> UpdateLoginFailAsync(string accId, int? failAttempts, DateTime? lockedUntil)
        {
            var filter = Builders<Account>.Filter.Eq(a => a.AccountId, accId);

            var update = Builders<Account>.Update
                .Set(a => a.FailedAttempts, failAttempts)
                .Set(a => a.LockedUntil, lockedUntil);

            // Update và trả về bản ghi sau khi update
            return await _collection.FindOneAndUpdateAsync(
                filter,
                update,
                new FindOneAndUpdateOptions<Account>
                {
                    ReturnDocument = ReturnDocument.After // trả về document sau khi update
                }
            );
        }

        public async Task<Account> UpdateRefreshTokenAsync(string? accId, string? refreshToken, DateTime? expiry)
        {
            var filter = Builders<Account>.Filter.Eq(a => a.AccountId, accId);

            var update = Builders<Account>.Update
                .Set(a => a.RefreshToken, refreshToken)
                .Set(a => a.TokenExpiry, expiry);

            return await _collection.FindOneAndUpdateAsync(
                filter,
                update,
                new FindOneAndUpdateOptions<Account>
                {
                    ReturnDocument = ReturnDocument.After
                }
            );
        }

        public async Task<List<Account>> GetAccountsByIdentifiersAsync(List<string>? accountIds)
        {
            if (accountIds == null || !accountIds.Any())
                return new List<Account>();

            var filter = Builders<Account>.Filter.In(a => a.AccountId, accountIds)
                & Builders<Account>.Filter.Eq(a => a.IsDeleted, false);

            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<Account?> SearchUserByIdentify(string? identify)
        {
            if (string.IsNullOrEmpty(identify))
                return null;

            identify = identify.Trim().Replace(" ", string.Empty);

            var filterIdentify = Builders<Account>.Filter.Or(
                Builders<Account>.Filter.Eq(u => u.Email, identify),
                Builders<Account>.Filter.Eq(u => u.PhoneNumber, identify)
            );

            var filterNotDeleted = Builders<Account>.Filter.Eq(u => u.IsDeleted, false);

            var finalFilter = Builders<Account>.Filter.And(filterIdentify, filterNotDeleted);


            return await _collection.Find(finalFilter).FirstOrDefaultAsync();
        }

    }
}
