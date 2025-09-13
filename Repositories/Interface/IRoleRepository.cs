using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models.Model;

namespace Repositories.Interface
{
    public interface IRoleRepository
    {
        Task<Role?> GetRoleById(string? roleId);
    }
}
