using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DTO.Response
{
    public class FriendListItemDTO
    {
        public string? FriendId { get; set; }       
        public ProfileResponseDTO? FriendInfor { get; set; }  
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
