using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models.DTO;
using Models.DTO.Request;
using Models.DTO.Response;

namespace Services.Interface
{
    public interface IChatService
    {
        Task<BaseResponseDTO<ChatResponseDTO>> CreateNewChat(CreateChatRequestDTO? request);
    }
}
