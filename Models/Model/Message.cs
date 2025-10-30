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
    public enum TypeMessage
    {
        Text,
        Image,
        OtherFile
    }
    public class Message
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string MessageId { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        [Required]
        public string ChatId { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        [Required]
        public string SenderId { get; set; }
        [Required]
        public TypeMessage Type { get; set; } = TypeMessage.Text;
        public string? TextMessage { get; set; }
        public List<FileMetadata>? Images { get; set; }
        public List<FileMetadata>? Files { get; set; }
        public DateTime CreatedAt { get; set; }
        [Required]
        public string Status { get; set; }

        public string? ReplyToMessageId { get; set; }
    }
}
