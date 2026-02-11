using MongoDB.Bson.Serialization.Attributes;

namespace AttendanceManagementSystem.Models.Entities
{
    public class AdminInvitation : BaseEntity
    {
        [BsonElement("email")]
        public string Email { get; set; } = string.Empty;

        [BsonElement("invitedRole")]
        public string InvitedRole { get; set; } = string.Empty;

        [BsonElement("token")]
        public string Token { get; set; } = string.Empty;

        [BsonElement("invitedBy")]
        public string InvitedBy { get; set; } = string.Empty;

        [BsonElement("invitedByName")]
        public string InvitedByName { get; set; } = string.Empty;

        [BsonElement("status")]
        public string Status { get; set; } = "Pending"; 

        [BsonElement("expiresAt")]
        public DateTime ExpiresAt { get; set; }

        [BsonElement("acceptedAt")]
        public DateTime? AcceptedAt { get; set; }

        [BsonElement("revokedAt")]
        public DateTime? RevokedAt { get; set; }

        [BsonElement("revokedBy")]
        public string? RevokedBy { get; set; }

        [BsonElement("notes")]
        public string? Notes { get; set; }
    }
}