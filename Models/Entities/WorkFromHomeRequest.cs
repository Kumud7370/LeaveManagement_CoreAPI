using AttendanceManagementSystem.Models.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AttendanceManagementSystem.Models.Entities
{
    public class WorkFromHomeRequest : BaseEntity
    {
        [BsonElement("employeeId")]
        public string EmployeeId { get; set; } = string.Empty;

        [BsonElement("startDate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime StartDate { get; set; }

        [BsonElement("endDate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime EndDate { get; set; }

        [BsonElement("reason")]
        public string Reason { get; set; } = string.Empty;

        [BsonRepresentation(BsonType.String)]
        public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;

        [BsonElement("approvedBy")]
        public string? ApprovedBy { get; set; }

        [BsonElement("approvedDate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? ApprovedDate { get; set; }

        [BsonElement("rejectionReason")]
        public string? RejectionReason { get; set; }

        [BsonElement("createdBy")]
        public string CreatedBy { get; set; } = string.Empty;

        [BsonElement("updatedBy")]
        public string? UpdatedBy { get; set; }

        [BsonElement("deletedAt")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? DeletedAt { get; set; }

        public int GetTotalDays()
        {
            return (EndDate.Date - StartDate.Date).Days + 1;
        }

        public bool IsActive()
        {
            var today = DateTime.UtcNow.Date;
            return Status == ApprovalStatus.Approved
                && StartDate.Date <= today
                && EndDate.Date >= today;
        }

        public bool IsPending()
        {
            return Status == ApprovalStatus.Pending;
        }

        public bool CanBeCancelled()
        {
            return Status == ApprovalStatus.Pending ||
                   (Status == ApprovalStatus.Approved && StartDate.Date > DateTime.UtcNow.Date);
        }
    }
}