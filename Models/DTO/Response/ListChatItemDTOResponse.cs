using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models.Model;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Models.DTO.Response
{
    public class ListChatItemDTOResponse
    {
        public string? ChatId { get; set; }
        public ChatType TypeChat { get; set; }
        public string? LastMessage { get; set; }
        public DateTime? LastMessageAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        //DÀNH CHO GROUP
        public string? GroupName { get; set; }
        public string? GroupAvatar { get; set; }
        public string? CreatedBy { get; set; }
        public bool IsDeleted { get; set; } = false;

        //Count unread message
        public int UnreadCountMessage { get; set; }
    }
}
