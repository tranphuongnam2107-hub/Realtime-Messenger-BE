using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models.Model;

namespace Models.DTO.Response
{
    public class ChatResponseDTO
    {
        public Chat? ChatInformation { get; set; }
        public List<ChatMember>? ChatMembers { get; set; }
    }
}
