using MongoDB.Bson.Serialization.Attributes;

namespace AttendanceManagementSystem.Models.Entities
{
    public class UserRole
    {
        [BsonElement("userId")]
        public string UserId { get; set; } = string.Empty;

        [BsonElement("roleId")]
        public string RoleId { get; set; } = string.Empty;

        [BsonElement("assignedAt")]
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    
}
}
