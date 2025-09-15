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

        public ChatRepository(ChatDAO chatDao)
        {
            _chatDao = chatDao;
        }

        public async Task CreateChatAsync(Chat chat)
        {
            await _chatDao.CreateChatAsync(chat);
        }

        public async Task<Chat> GetChatByIdAsync(string chatId)
        {
            return await _chatDao.GetChatByIdAsync(chatId);
        }

        public async Task UpdateLastMessageAsync(string chatId, string lastMessage)
        {
            await _chatDao.UpdateLastMessageAsync(chatId, lastMessage);
        }
    }
}
