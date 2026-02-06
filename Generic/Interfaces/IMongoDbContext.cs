using MongoDB.Driver;

namespace AttendanceManagementSystem.Data.Interfaces
{
    public interface IMongoDbContext
    {
        IMongoCollection<T> GetCollection<T>(string collectionName);
        IMongoDatabase Database { get; }
    }
}