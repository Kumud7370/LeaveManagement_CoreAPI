using AttendanceManagementSystem.Models.Enums;

namespace AttendanceManagementSystem.Models.DTOs.Attendance
{
    public class CheckOutDto
    {
        public string EmployeeId { get; set; } = string.Empty;
        public DateTime CheckOutTime { get; set; } = DateTime.UtcNow;
        public CheckInMethod CheckOutMethod { get; set; }
        public LocationDto? CheckOutLocation { get; set; }
        public string? CheckOutDeviceId { get; set; }
        public string? Remarks { get; set; }
    }
}