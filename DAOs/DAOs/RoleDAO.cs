using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using DataAccess.Context;
using Models.Model;
using MongoDB.Driver;

namespace DataAccess.DAOs
{
    public class RoleDAO : BaseDAO<Role>
    {
        public RoleDAO(MongoDBContext context) : base(context, "Role")
        {
        }

        public async Task<Role?> GetRoleByRoleId(string? roleId)
        {
            if (string.IsNullOrEmpty(roleId))
                return null;

            var filter = Builders<Role>.Filter.Eq(r => r.RoleId, roleId);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }
    }
}
