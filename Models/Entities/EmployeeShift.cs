using AttendanceManagementSystem.Models.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AttendanceManagementSystem.Models.Entities
{
    public class EmployeeShift : BaseEntity
    {
        [BsonElement("employeeId")]
        public string EmployeeId { get; set; } = string.Empty;

        [BsonElement("shiftId")]
        public string ShiftId { get; set; } = string.Empty;

        [BsonElement("effectiveFrom")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc, DateOnly = true)]
        public DateTime EffectiveFrom { get; set; }

        [BsonElement("effectiveTo")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc, DateOnly = true)]
        public DateTime? EffectiveTo { get; set; }

        [BsonElement("isActive")]
        public bool IsActive { get; set; } = true;

        [BsonElement("changeReason")]
        public string? ChangeReason { get; set; }

        [BsonRepresentation(BsonType.String)]
        public ShiftChangeStatus Status { get; set; } = ShiftChangeStatus.Pending;

        [BsonElement("requestedDate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime RequestedDate { get; set; }

        [BsonElement("approvedBy")]
        public string? ApprovedBy { get; set; }

        [BsonElement("approvedDate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? ApprovedDate { get; set; }

        [BsonElement("rejectedBy")]
        public string? RejectedBy { get; set; }

        [BsonElement("rejectedDate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? RejectedDate { get; set; }

        [BsonElement("rejectionReason")]
        public string? RejectionReason { get; set; }

        [BsonElement("createdBy")]
        public string CreatedBy { get; set; } = string.Empty;

        [BsonElement("updatedBy")]
        public string? UpdatedBy { get; set; }

        [BsonElement("deletedAt")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? DeletedAt { get; set; }

        public bool IsCurrentlyActive()
        {
            var today = DateTime.UtcNow.Date;
            return IsActive &&
                   Status == ShiftChangeStatus.Approved &&
                   EffectiveFrom.Date <= today &&
                   (!EffectiveTo.HasValue || EffectiveTo.Value.Date >= today);
        }

        public bool CanBeModified()
        {
            return Status == ShiftChangeStatus.Pending &&
                   EffectiveFrom > DateTime.UtcNow.Date;
        }

        public int GetDurationInDays()
        {
            if (!EffectiveTo.HasValue)
                return -1; 

            return (EffectiveTo.Value.Date - EffectiveFrom.Date).Days + 1;
        }

        public bool IsExpired()
        {
            return EffectiveTo.HasValue && EffectiveTo.Value.Date < DateTime.UtcNow.Date;
        }
    }
}