using AttendanceManagementSystem.Data.Interfaces;
using AttendanceManagementSystem.Models.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace AttendanceManagementSystem.Data.Implementations
{
    public class MongoDbContext : IMongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IOptions<MongoDbSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            _database = client.GetDatabase(settings.Value.DatabaseName);
        }

        public IMongoDatabase Database => _database;

        public IMongoCollection<T> GetCollection<T>(string collectionName)
        {
            return _database.GetCollection<T>(collectionName);
        }
    }
}