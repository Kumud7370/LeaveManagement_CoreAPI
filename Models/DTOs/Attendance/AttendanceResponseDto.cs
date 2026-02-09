using AttendanceManagementSystem.Models.Enums;

namespace AttendanceManagementSystem.Models.DTOs.Attendance
{
    public class AttendanceResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string EmployeeId { get; set; } = string.Empty;
        public string EmployeeCode { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public DateTime AttendanceDate { get; set; }
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public double? WorkingHours { get; set; }
        public double? OvertimeHours { get; set; }
        public AttendanceStatus Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public bool IsLate { get; set; }
        public bool IsEarlyLeave { get; set; }
        public int? LateMinutes { get; set; }
        public int? EarlyLeaveMinutes { get; set; }
        public LocationDto? CheckInLocation { get; set; }
        public LocationDto? CheckOutLocation { get; set; }
        public CheckInMethod? CheckInMethod { get; set; }
        public string? CheckInMethodName { get; set; }
        public CheckInMethod? CheckOutMethod { get; set; }
        public string? CheckOutMethodName { get; set; }
        public string? CheckInDeviceId { get; set; }
        public string? CheckOutDeviceId { get; set; }
        public string? Remarks { get; set; }
        public string? ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}