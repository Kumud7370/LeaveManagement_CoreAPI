namespace AttendanceManagementSystem.Models.DTOs.LeaveBalance
{
    public class BulkInitializeBalanceDto
    {
        public List<string> EmployeeIds { get; set; } = new List<string>();
        public int Year { get; set; }
        public bool IncludeCarryForward { get; set; } = true;
    }
}
