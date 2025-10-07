using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models.Model;

namespace Models.DTO.Response
{
    public class MessageResponseDTO
    {
        public string? ChatId { get; set; }
        public string? MessageId { get; set; }
        public ProfileResponseDTO? SenderInfor { get; set; }
        public string? Type { get; set; }
        public string? TextMessage { get; set; }
        public List<FileMetadata>? Images { get; set; }
        public List<FileMetadata>? Files { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Status { get; set; }

        public ReplyMessageDTO? ReplyToMessage { get; set; }

        public bool IsMine { get; set; } //kiểm tra tin nhắn của người dùng đang login hay không
    }
}
