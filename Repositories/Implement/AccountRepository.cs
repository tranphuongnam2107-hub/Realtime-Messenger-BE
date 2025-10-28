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
    public class AccountRepository : IAccountRepository
    {
        private readonly AccountDAO _accountDao;
        private readonly FriendDAO _friendDao;
        private readonly ChatMemberDAO _chatMemberDao;
        private readonly ChatDAO _chatDAO;

        public AccountRepository(AccountDAO accountDao, FriendDAO friendDao, ChatMemberDAO chatMemberDao, ChatDAO chatDAO)
        {
            _accountDao = accountDao;
            _friendDao = friendDao;
            _chatMemberDao = chatMemberDao;
            _chatDAO = chatDAO;
        }

        public async Task<Account?> GetAccountByIdentifier(string? identifier)
        {
            return await _accountDao.GetAccountByIdentify(identifier);
        }

        public async Task<List<Account>?> GetAccountsByIdentifier(List<string>? accountIds)
        {
            return await _accountDao.GetAccountsByIdentifiersAsync(accountIds);
        }

        public async Task<List<Account>> GetMutualFriend(string? loggedUserId, string? toUserId)
        {
            if (string.IsNullOrEmpty(loggedUserId) || string.IsNullOrEmpty(toUserId))
                return new List<Account>();

            var loggedUserFriends = await _friendDao.GetFriendIdsAsync(loggedUserId);
            var targetUserFriends = await _friendDao.GetFriendIdsAsync(toUserId);

            if (loggedUserFriends.Count == 0 || targetUserFriends.Count == 0)
                return new List<Account>();

            var mutualIds = loggedUserFriends.Intersect(targetUserFriends).ToList();

            if (mutualIds.Count == 0)
                return new List<Account>();

            var mutualAccounts = await _accountDao.GetAccountsByIdentifiersAsync(mutualIds);
            return mutualAccounts;
        }

        public async Task<List<Chat>> GetMutualGroup(string? loggedUserId, string? toUserId)
        {
            if (string.IsNullOrEmpty(loggedUserId) || string.IsNullOrEmpty(toUserId))
                return new List<Chat>();

            var loggedUserChatMembers = await _chatMemberDao.GetChatMembersOfUserAsync(loggedUserId);
            var targetUserChatMembers = await _chatMemberDao.GetChatMembersOfUserAsync(toUserId);

            if (loggedUserChatMembers == null || targetUserChatMembers == null)
                return new List<Chat>();

            var loggedUserGroupIds = loggedUserChatMembers
                .Select(cm => cm.ChatId)
                .Distinct()
                .ToList();

            var targetUserGroupIds = targetUserChatMembers
                .Select(cm => cm.ChatId)
                .Distinct()
                .ToList();

            if (loggedUserGroupIds.Count == 0 || targetUserGroupIds.Count == 0)
                return new List<Chat>();

            var mutualChatIds = loggedUserGroupIds.Intersect(targetUserGroupIds).ToList();

            if(mutualChatIds.Count == 0)
                return new List<Chat>();

            var mutualChats = await _chatDAO.GetChatsByIds(mutualChatIds);

            var mutualGroups = mutualChats.Where(c => c.TypeChat == ChatType.Group).ToList();

            return mutualGroups;
        }

        public async Task<Account?> SearchUserByIdentify(string? identify)
        {
            return await _accountDao.SearchUserByIdentify(identify);
        }

        public async Task<Account?> UpdateLoginFail(string accId, int? failAttempts, DateTime? lockedUntil)
        {
            return await _accountDao.UpdateLoginFailAsync(accId, failAttempts, lockedUntil);
        }

        public async Task<Account?> UpdateRefreshToken(string? accId, string? refreshToken, DateTime? expiry)
        {
            return await _accountDao.UpdateRefreshTokenAsync(accId, refreshToken, expiry);
        }
    }
}
