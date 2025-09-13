using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace DataAccess.Context
{
    public class MongoDBContext
    {
        public IMongoDatabase Database { get; }

        public MongoDBContext(MongoDBSetting setting)
        {
            if (setting == null)
                throw new ArgumentNullException(nameof(setting));

            var client = new MongoClient(setting.ConnectionString);
            Database = client.GetDatabase(setting.DatabaseName);
        }

        public IMongoCollection<T> GetCollection<T>(string name)
        {
            return Database.GetCollection<T>(name);
        }
    }
}
