using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Models.Model;

namespace Models.DTO.Request
{
    public class CreateChatRequestDTO
    {
        public ChatType ChatType { get; set; }

        //Field dành cho tạo group
        public string? GroupName { get; set; }
        public IFormFile? GroupAvatar { get; set; }
        

        //Member được add khi tạo group chat hoặc tạo private chat
        public List<ChatMemberRequestDTO>? ChatMembers { get; set; }
    }

}
