namespace AttendanceManagementSystem.Models.DTOs.LeaveBalance
{
    public class LeaveBalanceFilterDto
    {
        public string? EmployeeId { get; set; }
        public string? LeaveTypeId { get; set; }
        public int? Year { get; set; }
        public decimal? MinAvailableBalance { get; set; }
        public decimal? MaxAvailableBalance { get; set; }
        public decimal? MinConsumedBalance { get; set; }
        public decimal? MaxConsumedBalance { get; set; }
        public bool? IsLowBalance { get; set; }
        public decimal? LowBalanceThreshold { get; set; } = 2;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SortBy { get; set; } = "year";
        public bool SortDescending { get; set; } = true;
    }
}
