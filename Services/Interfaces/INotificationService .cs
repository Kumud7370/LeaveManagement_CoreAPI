using AttendanceManagementSystem.Models.DTOs.Notification;
using AttendanceManagementSystem.Models.Entities;

namespace AttendanceManagementSystem.Services.Interfaces
{
    public interface INotificationService
    {
        // ── Fetch ─────────────────────────────────────────────────────────────
        Task<List<NotificationDto>> GetMyNotificationsAsync(string userId);
        Task<UnreadCountDto> GetMyUnreadCountAsync(string userId);

        // ── Actions ───────────────────────────────────────────────────────────
        Task<bool> MarkAsReadAsync(string notificationId, string userId);
        Task<bool> MarkAllAsReadAsync(string userId);
        Task<bool> DeleteNotificationAsync(string notificationId, string userId);

        // ── Leave Notifications (called by LeaveService) ──────────────────────
        Task NotifyLeaveAppliedAsync(Leave leave, string employeeUserId);
        Task NotifyLeaveAdminApprovedAsync(Leave leave, string employeeUserId);
        Task NotifyLeaveNayabApprovedAsync(Leave leave, string employeeUserId);
        Task NotifyLeaveTehsildarApprovedAsync(Leave leave, string employeeUserId);
        Task NotifyLeaveRejectedAsync(Leave leave, string employeeUserId, string rejectionReason);
        Task NotifyLeaveCancelledAsync(Leave leave, string employeeUserId);

        // ── Department Notifications (called by DepartmentService) ────────────
        Task NotifyDepartmentCreatedAsync(string departmentId, string departmentName, string actorUserId, IEnumerable<string> recipientUserIds);
        Task NotifyDepartmentUpdatedAsync(string departmentId, string departmentName, string actorUserId, IEnumerable<string> recipientUserIds);
        Task NotifyDepartmentDeletedAsync(string departmentId, string departmentName, string actorUserId, IEnumerable<string> recipientUserIds);
        Task NotifyDepartmentStatusChangedAsync(string departmentId, string departmentName, bool isActive, string actorUserId, IEnumerable<string> recipientUserIds);

        // ── Designation Notifications (called by DesignationService) ──────────
        Task NotifyDesignationCreatedAsync(string designationId, string designationName, string actorUserId, IEnumerable<string> recipientUserIds);
        Task NotifyDesignationUpdatedAsync(string designationId, string designationName, string actorUserId, IEnumerable<string> recipientUserIds);
        Task NotifyDesignationDeletedAsync(string designationId, string designationName, string actorUserId, IEnumerable<string> recipientUserIds);
        Task NotifyDesignationStatusChangedAsync(string designationId, string designationName, bool isActive, string actorUserId, IEnumerable<string> recipientUserIds);

        // ── Background / Scheduled Jobs (called by WorkloadNotificationBackgroundService) ──
        Task SendOverdueAlertsAsync();
        Task SendWeeklySummaryAsync();
        Task CheckAndNotifyWorkloadAsync();
    }
}