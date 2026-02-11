using AttendanceManagementSystem.Models.Enums;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace AttendanceManagementSystem.Models.Entities
{
    public class Attendance : BaseEntity
    {
        [BsonElement("employeeId")]
        public string EmployeeId { get; set; } = string.Empty;

        [BsonElement("attendanceDate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime AttendanceDate { get; set; }

        [BsonElement("checkInTime")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? CheckInTime { get; set; }

        [BsonElement("checkOutTime")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? CheckOutTime { get; set; }

        [BsonElement("workingHours")]
        public double? WorkingHours { get; set; }

        [BsonElement("overtimeHours")]
        public double? OvertimeHours { get; set; }

        [BsonElement("status")]  // ADD THIS LINE
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public AttendanceStatus Status { get; set; }

        [BsonElement("isLate")]
        public bool IsLate { get; set; }

        [BsonElement("isEarlyLeave")]
        public bool IsEarlyLeave { get; set; }

        [BsonElement("lateMinutes")]
        public int? LateMinutes { get; set; }

        [BsonElement("earlyLeaveMinutes")]
        public int? EarlyLeaveMinutes { get; set; }

        [BsonElement("checkInLocation")]
        public Location? CheckInLocation { get; set; }

        [BsonElement("checkOutLocation")]
        public Location? CheckOutLocation { get; set; }

        [BsonElement("checkInMethod")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public CheckInMethod? CheckInMethod { get; set; }

        [BsonElement("checkOutMethod")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public CheckInMethod? CheckOutMethod { get; set; }

        [BsonElement("checkInDeviceId")]
        public string? CheckInDeviceId { get; set; }

        [BsonElement("checkOutDeviceId")]
        public string? CheckOutDeviceId { get; set; }

        [BsonElement("remarks")]
        public string? Remarks { get; set; }

        [BsonElement("approvedBy")]
        public string? ApprovedBy { get; set; }

        [BsonElement("approvedAt")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? ApprovedAt { get; set; }

        [BsonElement("createdBy")]
        public string CreatedBy { get; set; } = string.Empty;

        [BsonElement("updatedBy")]
        public string? UpdatedBy { get; set; }

        [BsonElement("deletedAt")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? DeletedAt { get; set; }

        public double CalculateWorkingHours()
        {
            if (CheckInTime.HasValue && CheckOutTime.HasValue)
            {
                var duration = CheckOutTime.Value - CheckInTime.Value;
                return Math.Round(duration.TotalHours, 2);
            }
            return 0;
        }

        public double CalculateOvertimeHours(double standardWorkingHours = 8.0)
        {
            if (WorkingHours.HasValue && WorkingHours.Value > standardWorkingHours)
            {
                return Math.Round(WorkingHours.Value - standardWorkingHours, 2);
            }
            return 0;
        }

        public bool IsFullDayPresent(double minimumWorkingHours = 8.0)
        {
            return WorkingHours.HasValue && WorkingHours.Value >= minimumWorkingHours;
        }

        public bool IsHalfDayPresent(double minimumHalfDayHours = 4.0, double minimumFullDayHours = 8.0)
        {
            return WorkingHours.HasValue &&
                   WorkingHours.Value >= minimumHalfDayHours &&
                   WorkingHours.Value < minimumFullDayHours;
        }
    }

    public class Location
    {
        [BsonElement("latitude")]
        public double Latitude { get; set; }

        [BsonElement("longitude")]
        public double Longitude { get; set; }

        [BsonElement("address")]
        public string? Address { get; set; }

        public Location()
        {
        }

        public Location(double latitude, double longitude, string? address = null)
        {
            Latitude = latitude;
            Longitude = longitude;
            Address = address;
        }

        public override string ToString()
        {
            return Address ?? $"Lat: {Latitude}, Lng: {Longitude}";
        }
    }
}