using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AttendanceManagementSystem.Models.Entities
{
    public class LeaveBalance : BaseEntity
    {
        [BsonElement("employeeId")]
        public string EmployeeId { get; set; } = string.Empty;

        [BsonElement("leaveTypeId")]
        public string LeaveTypeId { get; set; } = string.Empty;

        [BsonElement("year")]
        public int Year { get; set; }

        [BsonElement("totalAllocated")]
        public decimal TotalAllocated { get; set; }

        [BsonElement("consumed")]
        public decimal Consumed { get; set; }

        [BsonElement("carriedForward")]
        public decimal CarriedForward { get; set; }

        [BsonElement("available")]
        public decimal Available { get; set; }

        [BsonElement("lastUpdated")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        [BsonElement("createdBy")]
        public string CreatedBy { get; set; } = string.Empty;

        [BsonElement("updatedBy")]
        public string? UpdatedBy { get; set; }

        [BsonElement("deletedAt")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? DeletedAt { get; set; }

        public decimal GetAvailableBalance()
        {
            return TotalAllocated + CarriedForward - Consumed;
        }

        public void UpdateAvailableBalance()
        {
            Available = GetAvailableBalance();
            LastUpdated = DateTime.UtcNow;
        }

        
        public bool HasSufficientBalance(decimal requestedDays)
        {
            return GetAvailableBalance() >= requestedDays;
        }

        public bool ConsumeLeave(decimal days)
        {
            if (!HasSufficientBalance(days))
                return false;

            Consumed += days;
            UpdateAvailableBalance();
            return true;
        }

        public void RestoreLeave(decimal days)
        {
            Consumed = Math.Max(0, Consumed - days);
            UpdateAvailableBalance();
        }

        public bool IsCurrentYear()
        {
            return Year == DateTime.UtcNow.Year;
        }

        public decimal GetUtilizationPercentage()
        {
            var total = TotalAllocated + CarriedForward;
            return total > 0 ? (Consumed / total) * 100 : 0;
        }
    }
}
