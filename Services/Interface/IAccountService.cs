using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models.DTO;
using Models.DTO.Response;

namespace Services.Interface
{
    public interface IAccountService
    {
        Task<BaseResponseDTO<ProfileResponseDTO>> SearchUser(string? request);
    }
}
