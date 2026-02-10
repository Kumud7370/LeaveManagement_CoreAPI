using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AttendanceManagementSystem.Models.Entities
{
    public class Shift : BaseEntity
    {
        [BsonElement("shiftName")]
        public string ShiftName { get; set; } = string.Empty;

        [BsonElement("shiftCode")]
        public string ShiftCode { get; set; } = string.Empty;

        [BsonElement("startTime")]
        public TimeOnly StartTime { get; set; }

        [BsonElement("endTime")]
        public TimeOnly EndTime { get; set; }

        [BsonElement("gracePeriodMinutes")]
        public int GracePeriodMinutes { get; set; } = 0;

        [BsonElement("minimumWorkingMinutes")]
        public int MinimumWorkingMinutes { get; set; } = 480; // 8 hours default

        [BsonElement("breakDurationMinutes")]
        public int BreakDurationMinutes { get; set; } = 60;

        [BsonElement("isNightShift")]
        public bool IsNightShift { get; set; } = false;

        [BsonElement("isActive")]
        public bool IsActive { get; set; } = true;

        [BsonElement("displayOrder")]
        public int DisplayOrder { get; set; } = 0;

        [BsonElement("description")]
        public string Description { get; set; } = string.Empty;

        [BsonElement("color")]
        public string Color { get; set; } = "#000000";

        [BsonElement("nightShiftAllowancePercentage")]
        public decimal NightShiftAllowancePercentage { get; set; } = 0;

        [BsonElement("createdBy")]
        public string CreatedBy { get; set; } = string.Empty;

        [BsonElement("updatedBy")]
        public string? UpdatedBy { get; set; }

        [BsonElement("deletedAt")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? DeletedAt { get; set; }

        // Helper Methods
        public TimeSpan GetMinimumWorkingHours()
        {
            return TimeSpan.FromMinutes(MinimumWorkingMinutes);
        }

        public TimeSpan GetTotalShiftDuration()
        {
            var start = StartTime.ToTimeSpan();
            var end = EndTime.ToTimeSpan();

            if (end < start) // Night shift crosses midnight
            {
                return TimeSpan.FromDays(1) - start + end;
            }
            return end - start;
        }

        public bool IsWithinGracePeriod(TimeOnly actualTime)
        {
            var graceEndTime = StartTime.AddMinutes(GracePeriodMinutes);
            return actualTime <= graceEndTime;
        }

        public TimeSpan GetNetWorkingHours()
        {
            return GetTotalShiftDuration() - TimeSpan.FromMinutes(BreakDurationMinutes);
        }

        public string GetShiftTimingDisplay()
        {
            return $"{StartTime:HH:mm} - {EndTime:HH:mm}";
        }
    }
}