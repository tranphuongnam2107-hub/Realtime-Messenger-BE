using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Models.Model
{
    public enum ChatType
    {
        Private,
        Group
    }

    public class Chat
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ChatId { get; set; }
        [BsonRepresentation(BsonType.String)]
        public ChatType TypeChat { get; set; } = ChatType.Private;
        public string? LastMessage { get; set; }
        public DateTime? LastMessageAt { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        //DÀNH CHO GROUP
        public string? GroupName { get; set; }
        public string? GroupAvatar { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string? CreatedBy { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}
