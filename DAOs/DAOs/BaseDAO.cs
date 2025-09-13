using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess.Context;
using MongoDB.Driver;

namespace DataAccess.DAOs
{
    public abstract class BaseDAO<T>
    {
        protected readonly IMongoCollection<T> _collection;

        protected BaseDAO(MongoDBContext context, string collectionName)
        {
            _collection = context.GetCollection<T>(collectionName);
        }
    }
}
