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
    public class ChatRepository : IChatRepository
    {
        private readonly ChatDAO _chatDao;
        private readonly ChatMemberDAO _memberDao;

        public ChatRepository(ChatDAO chatDao, ChatMemberDAO memberDao)
        {
            _chatDao = chatDao;
            _memberDao = memberDao;
        }

        public async Task<Chat?> CreateChatAsync(Chat chat)
        {
            return await _chatDao.CreateChatAsync(chat);
        }

        public async Task<Chat> GetChatByIdAsync(string chatId)
        {
            return await _chatDao.GetChatByIdAsync(chatId);
        }

        public async Task UpdateLastMessageAsync(string chatId, string lastMessage)
        {
            await _chatDao.UpdateLastMessageAsync(chatId, lastMessage);
        }

        public async Task<ChatMember?> UpdateLastReadMessage(string memberId)
        {
            return await _memberDao.UpdateLastReadMessage(memberId);
        }

        public async Task<ChatMember?> AddNewMember(ChatMember request)
        {
            return await _memberDao.AddChatMember(request);
        }

        public async Task<List<Chat>?> GetChatsOfUser(string accountId)
        {
            var chatMembers = await _memberDao.GetChatMembersOfUserAsync(accountId);
            var chatIds = chatMembers.Select(cm => cm.ChatId).ToList();

            return await _chatDao.GetChatsByIds(chatIds);
        }

        public async Task IncreaseUnreadCountExceptSenderAsync(string chatId, string senderId)
        {
            await _memberDao.IncreaseUnreadCountExceptSenderAsync(chatId, senderId);
        }

        public async Task<List<ChatMember>> GetMembersByChatIdAsync(string chatId)
        {
            return await _memberDao.GetChatMemberByChatId(chatId);
        }

        public async Task<ChatMember> GetByChatAndUserIdAsync(string chatId, string accountId)
        {
            return await _memberDao.GetChatMemberAsync(chatId, accountId);
        }

        public async Task UpdateUnreadCount(string chatId, string accountId, int unreadCount)
        {
            await _memberDao.UpdateUnreadCountAsync(chatId, accountId, unreadCount);
        }
    }
}
