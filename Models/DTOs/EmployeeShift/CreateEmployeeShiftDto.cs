namespace AttendanceManagementSystem.Models.DTOs.EmployeeShift
{
    public class CreateEmployeeShiftDto
    {
        public string EmployeeId { get; set; } = string.Empty;
        public string ShiftId { get; set; } = string.Empty;
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public string? ChangeReason { get; set; }
    }
}