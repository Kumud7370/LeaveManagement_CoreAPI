using AttendanceManagementSystem.Models.Enums;
using System.Text.Json.Serialization;

namespace AttendanceManagementSystem.Models.DTOs.Holiday
{
    public class UpdateHolidayDto
    {
        public string? HolidayName { get; set; }
        public DateTime? HolidayDate { get; set; }
        public string? Description { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public HolidayType? HolidayType { get; set; }

        public bool? IsOptional { get; set; }
        public List<string>? ApplicableDepartments { get; set; }
        public bool? IsActive { get; set; }
    }
}








































