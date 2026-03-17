using AttendanceManagementSystem.Models.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AttendanceManagementSystem.Models.Entities
{
    public class Leave : BaseEntity
    {
        [BsonElement("employeeId")]
        public string EmployeeId { get; set; } = string.Empty;

        [BsonElement("leaveTypeId")]
        public string LeaveTypeId { get; set; } = string.Empty;

        [BsonElement("startDate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc, DateOnly = true)]
        public DateTime StartDate { get; set; }

        [BsonElement("endDate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc, DateOnly = true)]
        public DateTime EndDate { get; set; }

        [BsonElement("totalDays")]
        public decimal TotalDays { get; set; }

        [BsonElement("reason")]
        public string Reason { get; set; } = string.Empty;

        [BsonRepresentation(BsonType.String)]
        public LeaveStatus LeaveStatus { get; set; } = LeaveStatus.Pending;

        [BsonElement("appliedDate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime AppliedDate { get; set; }

        [BsonElement("approvedBy")]
        public string? ApprovedBy { get; set; }

        [BsonElement("approvedDate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? ApprovedDate { get; set; }
        public string? AdminApprovedBy { get; set; }
        public DateTime? AdminApprovedDate { get; set; }
        public string? NayabApprovedBy { get; set; }
        public DateTime? NayabApprovedDate { get; set; }
        public string? TehsildarApprovedBy { get; set; }
        public DateTime? TehsildarApprovedDate { get; set; }

        [BsonElement("rejectedBy")]
        public string? RejectedBy { get; set; }

        [BsonElement("rejectedDate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? RejectedDate { get; set; }

        [BsonElement("rejectionReason")]
        public string? RejectionReason { get; set; }

        [BsonElement("cancelledDate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? CancelledDate { get; set; }

        [BsonElement("cancellationReason")]
        public string? CancellationReason { get; set; }

        [BsonElement("isEmergencyLeave")]
        public bool IsEmergencyLeave { get; set; } = false;

        [BsonElement("attachmentUrl")]
        public string? AttachmentUrl { get; set; }

        [BsonElement("createdBy")]
        public string CreatedBy { get; set; } = string.Empty;

        [BsonElement("updatedBy")]
        public string? UpdatedBy { get; set; }

        [BsonElement("deletedAt")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? DeletedAt { get; set; }

        public int GetLeaveDuration()
        {
            return (int)Math.Ceiling(TotalDays);
        }

        public bool IsActiveLeave()
        {
            return LeaveStatus == LeaveStatus.FullyApproved &&
                   StartDate <= DateTime.UtcNow.Date &&
                   EndDate >= DateTime.UtcNow.Date;
        }

        public bool CanBeCancelled()
        {
            return LeaveStatus == LeaveStatus.Pending ||
                   LeaveStatus == LeaveStatus.AdminApproved ||
                   LeaveStatus == LeaveStatus.NayabApproved ||
                   (LeaveStatus == LeaveStatus.FullyApproved && StartDate > DateTime.UtcNow.Date);
        }
    }
}