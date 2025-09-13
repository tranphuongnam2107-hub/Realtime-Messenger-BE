using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DTO.Request
{
    public class LoginRequestDTO
    {
        public string? Identifier { get; set; }
        public string? Password { get; set; }
    }
}
