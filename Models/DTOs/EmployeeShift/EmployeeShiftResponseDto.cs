using AttendanceManagementSystem.Models.Enums;

namespace AttendanceManagementSystem.Models.DTOs.EmployeeShift
{
    public class EmployeeShiftResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string EmployeeId { get; set; } = string.Empty;
        public string? EmployeeCode { get; set; }
        public string? EmployeeName { get; set; }
        public string ShiftId { get; set; } = string.Empty;
        public string? ShiftName { get; set; }
        public string? ShiftCode { get; set; }
        public string? ShiftColor { get; set; }
        public TimeOnly? ShiftStartTime { get; set; }
        public TimeOnly? ShiftEndTime { get; set; }
        public string? ShiftTimingDisplay { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public bool IsActive { get; set; }
        public string? ChangeReason { get; set; }
        public ShiftChangeStatus Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public DateTime RequestedDate { get; set; }
        public string? ApprovedBy { get; set; }
        public string? ApprovedByName { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public string? RejectedBy { get; set; }
        public string? RejectedByName { get; set; }
        public DateTime? RejectedDate { get; set; }
        public string? RejectionReason { get; set; }
        public bool IsCurrentlyActive { get; set; }
        public bool CanBeModified { get; set; }
        public int? DurationInDays { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}