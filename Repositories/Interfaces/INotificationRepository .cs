using AttendanceManagementSystem.Models.Entities;

namespace AttendanceManagementSystem.Repositories.Interfaces
{
    public interface INotificationRepository
    {
        Task<NotificationItem> CreateAsync(NotificationItem notification);

        /// <summary>All undeleted notifications for a user, newest first.</summary>
        Task<List<NotificationItem>> GetByUserIdAsync(string userId);

        Task<int> GetUnreadCountAsync(string userId);

        Task<bool> MarkAsReadAsync(string notificationId, string userId);

        Task<bool> MarkAllAsReadAsync(string userId);

        /// <summary>Soft-delete a single notification.</summary>
        Task<bool> DeleteAsync(string notificationId, string userId);

        Task<NotificationItem?> GetByIdAsync(string id);
    }
}