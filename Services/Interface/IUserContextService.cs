using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interface
{
    public interface IUserContextService
    {
        string? GetAccountIdFromToken();
        string? GetEmailFromToken();
        string? GetPhoneFromToken();
        string? GetRoleIdFromToken();
        string? GetRoleNameFromToken();
    }
}
