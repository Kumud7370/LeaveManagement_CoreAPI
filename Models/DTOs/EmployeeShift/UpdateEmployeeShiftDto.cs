namespace AttendanceManagementSystem.Models.DTOs.EmployeeShift
{
    public class UpdateEmployeeShiftDto
    {
        public string? ShiftId { get; set; }
        public DateTime? EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public string? ChangeReason { get; set; }
    }
}