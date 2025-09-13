using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess.DAOs;
using Models.Model;
using Repositories.Interface;

namespace Repositories.Implement
{
    public class RoleRepository : IRoleRepository
    {
        private readonly RoleDAO _roleDao;

        public RoleRepository(RoleDAO roleDao)
        {
            _roleDao = roleDao;
        }

        public Task<Role?> GetRoleById(string? roleId)
        {
            return _roleDao.GetRoleByRoleId(roleId);
        }
    }
}
