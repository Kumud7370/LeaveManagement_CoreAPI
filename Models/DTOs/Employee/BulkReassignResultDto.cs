namespace AttendanceManagementSystem.Models.DTOs.Employee
{
    public class BulkReassignResultDto
    {
        public int TotalRequested { get; set; }
        public int Succeeded { get; set; }
        public int Failed { get; set; }
        public List<string> FailedIds { get; set; } = new();

        public string Message => Failed == 0
            ? $"All {Succeeded} employee(s) reassigned successfully."
            : $"{Succeeded} succeeded, {Failed} failed.";
    }
}
