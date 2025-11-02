using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models.Model;

namespace Repositories.Interface
{
    public interface IFriendRepository
    {
        Task<Friend?> AddNewFriend(Friend? friend);
        Task<Friend?> UpdateStatusFriend(string? friendId, StatusFriend newStatus);
        Task<Friend?> GetFriendByUsers(string? firstUser, string? secondUser);
        Task<Friend?> GetFriendByFriendId(string? friendId);
        Task<List<Friend>> GetIncomingFriendRequests(string? accountId);
        Task<List<Friend>> GetOutgoingFriendRequests(string accountId);
        Task<List<Friend>> GetFriendsOfUser(string? accountId);
    }
}
