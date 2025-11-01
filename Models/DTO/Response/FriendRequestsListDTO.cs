using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DTO.Response
{
    public class FriendRequestsListDTO
    {
        public List<FriendResponseDTO>? IncomingRequest { get; set; } = new();
        public List<FriendResponseDTO>? OutgoingRequest { get; set; } = new();
    }
}
