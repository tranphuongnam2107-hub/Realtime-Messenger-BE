using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Services.Interface;

namespace Services.Implement
{
    public class UserContextService : IUserContextService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserContextService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string? GetAccountIdFromToken()
        {
            return _httpContextAccessor.HttpContext?.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        }

        public string? GetEmailFromToken()
        {
            return _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Email)?.Value;
        }

        public string? GetPhoneFromToken()
        {
            return _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.MobilePhone)?.Value;
        }

        public string? GetRoleIdFromToken()
        {
            return _httpContextAccessor.HttpContext?.User.FindFirst("RoleId")?.Value;
        }

        public string? GetRoleNameFromToken()
        {
            return _httpContextAccessor.HttpContext?.User.FindFirst("RoleName")?.Value;
        }
    }
}
