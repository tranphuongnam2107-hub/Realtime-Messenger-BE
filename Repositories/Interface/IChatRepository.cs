using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models.Model;

namespace Repositories.Interface
{
    public interface IChatRepository
    {
        Task<Chat> GetChatByIdAsync(string chatId);
        Task UpdateLastMessageAsync(string chatId, string lastMessage);
        Task<Chat?> CreateChatAsync(Chat chat);
        Task<List<Chat>?> GetChatsOfUser(string accountId);

        //CHAT MEMBER
        Task<ChatMember?> AddNewMember(ChatMember request);
        Task<ChatMember?> UpdateLastReadMessage(string memberId);
        Task IncreaseUnreadCountExceptSenderAsync(string chatId, string senderId);
        Task<List<ChatMember>> GetMembersByChatIdAsync(string chatId);
        Task<ChatMember> GetByChatAndUserIdAsync(string chatId, string accountId);
        Task UpdateUnreadCount(string chatId, string accountId, int unreadCount);
    }
}
