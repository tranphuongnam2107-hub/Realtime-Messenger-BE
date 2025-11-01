using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models.Model;

namespace Models.DTO.Request
{
    public class ResponseFriendRequestDTO
    {
        public string? FriendId { get; set; }
        public StatusFriend NewStatus { get; set; }
    }
}
