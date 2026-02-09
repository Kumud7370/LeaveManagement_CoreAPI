namespace AttendanceManagementSystem.Models.DTOs.Attendance
{
    public class AttendanceSummaryDto
    {
        public string EmployeeId { get; set; } = string.Empty;
        public string EmployeeCode { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalWorkingDays { get; set; }
        public int PresentDays { get; set; }
        public int AbsentDays { get; set; }
        public int HalfDays { get; set; }
        public int LeaveDays { get; set; }
        public int Holidays { get; set; }
        public int WeekOffs { get; set; }
        public int LateDays { get; set; }
        public int EarlyLeaveDays { get; set; }
        public double TotalWorkingHours { get; set; }
        public double TotalOvertimeHours { get; set; }
        public double AverageWorkingHours { get; set; }
        public double AttendancePercentage { get; set; }
    }
}