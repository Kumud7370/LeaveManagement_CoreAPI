using AttendanceManagementSystem.Data.Interfaces;
using AttendanceManagementSystem.Models.Entities;
using MongoDB.Driver;

namespace AttendanceManagementSystem.Repositories.Implementations
{
    public abstract class BaseRepository<T> where T : BaseEntity
    {
        protected readonly IMongoCollection<T> _collection;

        protected BaseRepository(IMongoDbContext context)
        {
            var collectionName = typeof(T).Name.ToLower() + "s";
            _collection = context.GetCollection<T>(collectionName);
        }

        public virtual async Task<T> CreateAsync(T entity)
        {
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;
            await _collection.InsertOneAsync(entity);
            return entity;
        }

        public virtual async Task<T?> GetByIdAsync(string id)
        {
            return await _collection.Find(x => x.Id == id && !x.IsDeleted).FirstOrDefaultAsync();
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _collection.Find(x => !x.IsDeleted).ToListAsync();
        }

        public virtual async Task<bool> UpdateAsync(string id, T entity)
        {
            entity.UpdatedAt = DateTime.UtcNow;
            var result = await _collection.ReplaceOneAsync(x => x.Id == id && !x.IsDeleted, entity);
            return result.ModifiedCount > 0;
        }

        public virtual async Task<bool> DeleteAsync(string id)
        {
            var update = Builders<T>.Update
                .Set(x => x.IsDeleted, true)
                .Set(x => x.UpdatedAt, DateTime.UtcNow);

            var result = await _collection.UpdateOneAsync(x => x.Id == id, update);
            return result.ModifiedCount > 0;
        }
    }
}