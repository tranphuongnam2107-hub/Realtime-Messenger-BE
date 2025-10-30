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
    public class FriendRepository : IFriendRepository
    {
        private readonly FriendDAO _friendDao;

        public FriendRepository(FriendDAO friendDao)
        {
            _friendDao = friendDao;
        }

        public async Task<Friend?> AddNewFriend(Friend? friend)
        {
            return await _friendDao.AddFriend(friend);
        }

        public async Task<Friend?> GetFriendByFriendId(string? friendId)
        {
            return await _friendDao.GetFriendById(friendId);
        }

        public async Task<Friend?> GetFriendByUsers(string? firstUser, string? secondUser)
        {
            return await _friendDao.GetFriendByUsers(firstUser, secondUser);
        }

        public async Task<Friend?> UpdateStatusFriend(string? friendId, StatusFriend newStatus)
        {
            return await _friendDao.UpdateStatusFriend(friendId, newStatus);
        }
    }
}
