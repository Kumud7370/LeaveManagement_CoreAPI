namespace AttendanceManagementSystem.Models.DTOs.Common
{
    public class ErrorResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public string? StackTrace { get; set; }
        public int StatusCode { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
