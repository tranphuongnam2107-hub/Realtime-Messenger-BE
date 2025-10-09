using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models.Model;

namespace Models.DTO.Request
{
    public class ChatMemberRequestDTO
    {
        public string? AccountId { get; set; }
        public ChatRole RoleDescription { get; set; }
    }
}
