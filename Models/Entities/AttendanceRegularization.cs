using AttendanceManagementSystem.Models.Enums;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace AttendanceManagementSystem.Models.Entities
{
    public class AttendanceRegularization : BaseEntity
    {
        [BsonElement("attendanceId")]
        public string? AttendanceId { get; set; }

        [BsonElement("employeeId")]
        public string EmployeeId { get; set; } = string.Empty;

        [BsonElement("attendanceDate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime AttendanceDate { get; set; }

        [BsonElement("regularizationType")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public RegularizationType RegularizationType { get; set; }

        [BsonElement("requestedCheckIn")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? RequestedCheckIn { get; set; }

        [BsonElement("requestedCheckOut")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? RequestedCheckOut { get; set; }

        [BsonElement("originalCheckIn")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? OriginalCheckIn { get; set; }

        [BsonElement("originalCheckOut")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? OriginalCheckOut { get; set; }

        [BsonElement("reason")]
        public string Reason { get; set; } = string.Empty;

        [BsonElement("status")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public RegularizationStatus Status { get; set; } = RegularizationStatus.Pending;

        [BsonElement("approvedBy")]
        public string? ApprovedBy { get; set; }

        [BsonElement("approvedAt")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? ApprovedAt { get; set; }

        [BsonElement("rejectionReason")]
        public string? RejectionReason { get; set; }

        [BsonElement("requestedBy")]
        public string RequestedBy { get; set; } = string.Empty;

        [BsonElement("requestedAt")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedBy")]
        public string? UpdatedBy { get; set; }

       
        public bool CanBeApproved()
        {
            return Status == RegularizationStatus.Pending;
        }

        public bool IsWithinAllowedPeriod(int maxDaysBack = 7)
        {
            var daysDifference = (DateTime.UtcNow.Date - AttendanceDate.Date).Days;
            return daysDifference <= maxDaysBack;
        }

        public bool IsValidTimeRange()
        {
            if (RequestedCheckIn.HasValue && RequestedCheckOut.HasValue)
            {
                return RequestedCheckOut.Value > RequestedCheckIn.Value;
            }
            return true;
        }
    }
}