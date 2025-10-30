using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models.Model;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Models.DTO.Response
{
    public class FriendResponseDTO
    {
        public string? FriendId { get; set; }
        public ProfileResponseDTO? SenderInfor { get; set; }
        public ProfileResponseDTO? ReceiverInfor { get; set; }
        public StatusFriend? StatusFriend { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
