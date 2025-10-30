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
    public enum StatusFriend
    {
        Accepted,
        Requested
    }
    public class Friend
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string FriendId { get; set; }
        [Required]
        [BsonRepresentation(BsonType.ObjectId)]
        public string SenderId { get; set; }
        [Required]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ReceiverId { get; set; }
        [Required]
        public StatusFriend StatusFriend { get; set; } = StatusFriend.Requested;
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
