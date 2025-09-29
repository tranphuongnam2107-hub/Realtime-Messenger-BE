using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Config.JwtConfig
{
    public class JwtSetting
    {
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public int TokenValidMins { get; set; } = 15;
        public int RefreshTokenValidMins { get; set; } = 10080;
    }
}
