namespace AttendanceManagementSystem.Models.DTOs.LeaveBalance
{
    public class CollectiveLeaveBalancedto
    {
        public string? LeaveTypeId { get; set; }
        public List<string> EmployeeIds { get; set; } = new List<string>();
        public int Year { get; set; }
        public decimal? TotalAllocated { get; set; }
        public decimal? CarriedForward { get; set; }
        public bool SkipExisting { get; set; } = true;
    }
}
