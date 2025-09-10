using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess.Context;
using Models.Model;
using MongoDB.Driver;

namespace DataAccess.DAOs
{
    public class AccountDAO : BaseDAO<Account>
    {
        public AccountDAO(MongoDBContext context) : base(context, "Account")
        {
        }

        public async Task<Account?> GetByEmailAsync(string? email)
        {
            return null;
        }
    }
}
