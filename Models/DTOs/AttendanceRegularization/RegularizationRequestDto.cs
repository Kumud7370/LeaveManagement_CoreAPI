using AttendanceManagementSystem.Models.Enums;

namespace AttendanceManagementSystem.Models.DTOs.AttendanceRegularization
{
    public class RegularizationRequestDto
    {
        public string EmployeeId { get; set; } = string.Empty;
        public DateTime AttendanceDate { get; set; }
        public RegularizationType RegularizationType { get; set; }
        public DateTime? RequestedCheckIn { get; set; }
        public DateTime? RequestedCheckOut { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}