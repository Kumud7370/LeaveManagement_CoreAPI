using AttendanceManagementSystem.Data.Interfaces;
using AttendanceManagementSystem.Models.Entities;
using AttendanceManagementSystem.Repositories.Interfaces;
using MongoDB.Driver;

namespace AttendanceManagementSystem.Repositories.Implementations
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly IMongoCollection<NotificationItem> _collection;

        public NotificationRepository(IMongoDbContext context)
        {
            _collection = context.GetCollection<NotificationItem>("notifications");
        }

        public async Task<NotificationItem> CreateAsync(NotificationItem notification)
        {
            await _collection.InsertOneAsync(notification);
            return notification;
        }

        public async Task<List<NotificationItem>> GetByUserIdAsync(string userId)
        {
            return await _collection
                .Find(n => n.RecipientUserId == userId && !n.IsDeleted)
                .SortByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return (int)await _collection
                .CountDocumentsAsync(n => n.RecipientUserId == userId && !n.IsRead && !n.IsDeleted);
        }

        public async Task<bool> MarkAsReadAsync(string notificationId, string userId)
        {
            var update = Builders<NotificationItem>.Update
                .Set(n => n.IsRead, true)
                .Set(n => n.ReadAt, DateTime.UtcNow);

            var result = await _collection.UpdateOneAsync(
                n => n.Id == notificationId && n.RecipientUserId == userId && !n.IsDeleted,
                update);

            return result.ModifiedCount > 0;
        }

        public async Task<bool> MarkAllAsReadAsync(string userId)
        {
            var update = Builders<NotificationItem>.Update
                .Set(n => n.IsRead, true)
                .Set(n => n.ReadAt, DateTime.UtcNow);

            var result = await _collection.UpdateManyAsync(
                n => n.RecipientUserId == userId && !n.IsRead && !n.IsDeleted,
                update);

            return result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteAsync(string notificationId, string userId)
        {
            var update = Builders<NotificationItem>.Update
                .Set(n => n.IsDeleted, true)
                .Set(n => n.DeletedAt, DateTime.UtcNow);

            var result = await _collection.UpdateOneAsync(
                n => n.Id == notificationId && n.RecipientUserId == userId,
                update);

            return result.ModifiedCount > 0;
        }

        public async Task<NotificationItem?> GetByIdAsync(string id)
        {
            return await _collection
                .Find(n => n.Id == id && !n.IsDeleted)
                .FirstOrDefaultAsync();
        }
    }
}