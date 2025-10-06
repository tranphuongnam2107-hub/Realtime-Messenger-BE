using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DTO.Response
{
    public class UserClaimsResponseDTO
    {
        public string? AccountId { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? RoleId { get; set; }
        public string? RoleName { get; set; }
    }
}
