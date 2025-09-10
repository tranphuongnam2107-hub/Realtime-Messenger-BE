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

        public virtual async Task<List<T>> GetAllAsync()
        {
            return await _collection.Find(_ => true).ToListAsync();
        }

        public virtual async Task<T?> GetByIdAsync(string id)
        {
            return await _collection.Find(Builders<T>.Filter.Eq("_id", id)).FirstOrDefaultAsync();
        }
    }
}
