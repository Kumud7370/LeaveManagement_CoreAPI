namespace AttendanceManagementSystem.Models.DTOs.LeaveBalance
{
    public class CollectiveAssignmentResultDto
    {
        public int Succeeded { get; set; }
        public int Skipped { get; set; }
        public int Failed { get; set; }
        public List<string> FailedEmployeeIds { get; set; } = new List<string>();
        public List<string> SkippedEmployeeIds { get; set; } = new List<string>();
    }
}
