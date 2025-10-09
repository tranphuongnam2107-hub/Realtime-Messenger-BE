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
    public enum ChatRole
    {
        AdminGroup,
        Member,
        SubAdminGroup
    }

    public class ChatMember
    {
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string MemberId { get; set; }
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string ChatId { get; set; }
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string AccountId { get; set; }
        [Required]
        [BsonRepresentation(BsonType.String)]
        public ChatRole RoleDescription { get; set; } = ChatRole.Member;
        [Required]
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastReadAt { get; set; }
    }
}
