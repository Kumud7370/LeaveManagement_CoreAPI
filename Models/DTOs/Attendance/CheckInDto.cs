using AttendanceManagementSystem.Models.Enums;

namespace AttendanceManagementSystem.Models.DTOs.Attendance
{
    public class CheckInDto
    {
        public string EmployeeId { get; set; } = string.Empty;
        public DateTime CheckInTime { get; set; } = DateTime.UtcNow;
        public CheckInMethod CheckInMethod { get; set; }
        public LocationDto? CheckInLocation { get; set; }
        public string? CheckInDeviceId { get; set; }
        public string? Remarks { get; set; }
    }

    public class LocationDto
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? Address { get; set; }
    }
}