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
        Task CreateChatAsync(Chat chat);
    }
}
