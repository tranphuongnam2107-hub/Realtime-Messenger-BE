using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models.Model;

namespace Models.DTO.Response
{
    public class ReplyMessageDTO
    {
        public string? MessageId { get; set; }
        public string? SenderId { get; set; }
        public string? SenderName { get; set; } 
        public TypeMessage? Type { get; set; }      
        public string? TextMessage { get; set; }
        public List<FileMetadata>? Images { get; set; }
        public List<FileMetadata>? Files { get; set; }
    }
}
