using MongoDB.Bson.Serialization.Attributes;

namespace AttendanceManagementSystem.Models.Entities
{
    public class User : BaseEntity
    {
        [BsonElement("username")]
        public string Username { get; set; } = string.Empty;
        [BsonElement("employeeId")]
        public string? EmployeeId { get; set; }

        [BsonElement("email")]
        public string Email { get; set; } = string.Empty;

        [BsonElement("passwordHash")]
        public string PasswordHash { get; set; } = string.Empty;

        [BsonElement("firstName")]
        public string FirstName { get; set; } = string.Empty;

        [BsonElement("lastName")]
        public string LastName { get; set; } = string.Empty;

        [BsonElement("isActive")]
        public bool IsActive { get; set; } = true;

        [BsonElement("roleIds")]
        public List<string> RoleIds { get; set; } = new List<string>();
        
        [BsonElement("departmentId")]
        public string? DepartmentId { get; set; }

        [BsonElement("refreshToken")]
        public string? RefreshToken { get; set; }

        [BsonElement("refreshTokenExpiryTime")]
        public DateTime? RefreshTokenExpiryTime { get; set; }

        
    }
}