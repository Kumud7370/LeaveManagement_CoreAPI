using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AttendanceManagementSystem.Models.Entities
{
   
    public enum NotificationType
    {
        LeaveApplied,
        LeaveAdminApproved,
        LeaveNayabApproved,
        LeaveTehsildarApproved,   
        LeaveRejected,
        LeaveCancelled,
        DepartmentCreated,
        DepartmentUpdated,
        DepartmentDeleted,
        DepartmentStatusChanged,
        DesignationCreated,
        DesignationUpdated,
        DesignationDeleted,
        DesignationStatusChanged
    }

    public class NotificationItem
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        public string RecipientUserId { get; set; } = string.Empty;

        public string? RecipientEmployeeId { get; set; }

        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;

        [BsonRepresentation(BsonType.String)]
        public NotificationType Type { get; set; }

        public string? ReferenceId { get; set; }

        public bool IsRead { get; set; } = false;
        public DateTime? ReadAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; }

        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
    }
}