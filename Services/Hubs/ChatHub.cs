using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Services.Hubs
{
    public class ChatHub : Hub
    {
        // Tham gia vào nhóm chat
        public async Task JoinChatGroup(string chatId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, chatId);
            await Clients.Group(chatId).SendAsync("UserJoined", Context.ConnectionId);
        }

        // Thoát nhóm
        public async Task LeaveChatGroup(string chatId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatId);
            await Clients.Group(chatId).SendAsync("UserLeft", Context.ConnectionId);
        }

        // Typing indicator
        public async Task Typing(string chatId, string userId)
        {
            await Clients.Group(chatId).SendAsync("UserTyping", new { ChatId = chatId, UserId = userId });
        }

        public async Task StopTyping(string chatId, string userId)
        {
            await Clients.Group(chatId).SendAsync("UserStopTyping", new { ChatId = chatId, UserId = userId });
        }
    }
}
