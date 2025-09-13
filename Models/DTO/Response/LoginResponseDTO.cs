using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DTO.Response
{
    public class LoginResponseDTO
    {
        public string? AccountId { get; set; }
        public string? RoleId { get; set; }
        public string? AccessToken { get; set; }
        public int TokenExpiryIn { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? LockedUntil { get; set; }
    }
}
