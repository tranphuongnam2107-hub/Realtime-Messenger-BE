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
    public class Chat
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ChatId { get; set; }
        [Required]
        public string TypeChat { get; set; }
        public string LastMessage { get; set; }
        public DateTime LastMessageAt { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; }

        //DÀNH CHO GROUP
        public string GroupName { get; set; }
        public string GroupAvatar { get; set; }
        public string CreatedBy { get; set; }
    }
}
