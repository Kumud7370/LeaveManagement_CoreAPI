using AttendanceManagementSystem.Models.Enums;
using System.Text.Json.Serialization;

namespace AttendanceManagementSystem.Models.DTOs.Holiday
{
    public class HolidayFilterDto
    {
        public string? SearchTerm { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public HolidayType? HolidayType { get; set; }
        public bool? IsOptional { get; set; }
        public string? DepartmentId { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public bool? IsUpcoming { get; set; }
        public int? Year { get; set; }
        public int? Month { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SortBy { get; set; } = "HolidayDate";
        public bool SortDescending { get; set; } = false;
    }
}