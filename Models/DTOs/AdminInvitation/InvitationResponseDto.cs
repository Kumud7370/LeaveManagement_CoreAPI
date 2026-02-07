namespace AttendanceManagementSystem.Models.DTOs.AdminManagement
{
    public class InvitationResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string InvitedRole { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string InvitedBy { get; set; } = string.Empty;
        public string InvitedByName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? AcceptedAt { get; set; }
        public DateTime? RevokedAt { get; set; }
        public string? RevokedBy { get; set; }
        public string? Notes { get; set; }
    }
}
