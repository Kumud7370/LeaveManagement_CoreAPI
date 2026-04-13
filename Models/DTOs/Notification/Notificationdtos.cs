namespace AttendanceManagementSystem.Models.DTOs.Notification
{
    public class NotificationDto
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string? ReferenceId { get; set; }

        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReadAt { get; set; }
    }

    public class UnreadCountDto
    {
        public int UnreadCount { get; set; }
    }

    public class CreateNotificationDto
    {
        public string RecipientUserId { get; set; } = string.Empty;

        public string? RecipientEmployeeId { get; set; }

        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public string? ReferenceId { get; set; }
    }
}