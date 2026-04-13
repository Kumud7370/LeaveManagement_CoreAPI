//using AttendanceManagementSystem.Models.DTOs.Notification;
//using AttendanceManagementSystem.Models.Entities;
//using AttendanceManagementSystem.Models.Enums;
//using AttendanceManagementSystem.Repositories.Interfaces;
//using AttendanceManagementSystem.Services.Interfaces;

//namespace AttendanceManagementSystem.Services.Implementations
//{
//    public class NotificationService : INotificationService
//    {
//        private readonly INotificationRepository _notificationRepository;
//        private readonly ILeaveRepository _leaveRepository;
//        private readonly ILeaveBalanceRepository _leaveBalanceRepository;
//        private readonly IEmployeeRepository _employeeRepository;
//        private readonly IUserRepository _userRepository;
//        private readonly IRoleRepository _roleRepository;
//        private readonly IDepartmentRepository _departmentRepository;
//        private readonly ILogger<NotificationService> _logger;

//        private const double WorkloadThresholdPercent = 0.5;

//        public NotificationService(
//            INotificationRepository notificationRepository,
//            ILeaveRepository leaveRepository,
//            ILeaveBalanceRepository leaveBalanceRepository,
//            IEmployeeRepository employeeRepository,
//            IUserRepository userRepository,
//            IRoleRepository roleRepository,
//            IDepartmentRepository departmentRepository,
//            ILogger<NotificationService> logger)
//        {
//            _notificationRepository = notificationRepository;
//            _leaveRepository = leaveRepository;
//            _leaveBalanceRepository = leaveBalanceRepository;
//            _employeeRepository = employeeRepository;
//            _userRepository = userRepository;
//            _roleRepository = roleRepository;
//            _departmentRepository = departmentRepository;
//            _logger = logger;
//        }

//        // ── Fetch ─────────────────────────────────────────────────────────────

//        public async Task<List<NotificationDto>> GetMyNotificationsAsync(string userId)
//        {
//            var items = await _notificationRepository.GetByUserIdAsync(userId);
//            return items.Select(MapToDto).ToList();
//        }

//        public async Task<UnreadCountDto> GetMyUnreadCountAsync(string userId)
//        {
//            var count = await _notificationRepository.GetUnreadCountAsync(userId);
//            return new UnreadCountDto { UnreadCount = count };
//        }

//        // ── Actions ───────────────────────────────────────────────────────────

//        public Task<bool> MarkAsReadAsync(string notificationId, string userId)
//            => _notificationRepository.MarkAsReadAsync(notificationId, userId);

//        public Task<bool> MarkAllAsReadAsync(string userId)
//            => _notificationRepository.MarkAllAsReadAsync(userId);

//        public Task<bool> DeleteNotificationAsync(string notificationId, string userId)
//            => _notificationRepository.DeleteAsync(notificationId, userId);

//        // ── Leave Notifications ───────────────────────────────────────────────

//        public Task NotifyLeaveAppliedAsync(Leave leave, string employeeUserId)
//            => SaveAsync(employeeUserId, leave.EmployeeId,
//                "Leave Application Submitted",
//                $"Your leave from {leave.StartDate:dd MMM yyyy} to {leave.EndDate:dd MMM yyyy} ({leave.TotalDays} day(s)) has been submitted and is pending approval.",
//                NotificationType.LeaveApplied, leave.Id);

//        public Task NotifyLeaveAdminApprovedAsync(Leave leave, string employeeUserId)
//            => SaveAsync(employeeUserId, leave.EmployeeId,
//                "Leave Approved by Admin",
//                $"Your leave from {leave.StartDate:dd MMM yyyy} to {leave.EndDate:dd MMM yyyy} has been approved by the Admin. Awaiting Nayab approval.",
//                NotificationType.LeaveAdminApproved, leave.Id);

//        public Task NotifyLeaveNayabApprovedAsync(Leave leave, string employeeUserId)
//            => SaveAsync(employeeUserId, leave.EmployeeId,
//                "Leave Approved by Nayab",
//                $"Your leave from {leave.StartDate:dd MMM yyyy} to {leave.EndDate:dd MMM yyyy} has been approved by the Nayab. Awaiting Tehsildar final approval.",
//                NotificationType.LeaveNayabApproved, leave.Id);

//        public Task NotifyLeaveTehsildarApprovedAsync(Leave leave, string employeeUserId)
//            => SaveAsync(employeeUserId, leave.EmployeeId,
//                "Leave Fully Approved",
//                $"Your leave from {leave.StartDate:dd MMM yyyy} to {leave.EndDate:dd MMM yyyy} ({leave.TotalDays} day(s)) has been fully approved by the Tehsildar.",
//                NotificationType.LeaveTehsildarApproved, leave.Id);

//        public Task NotifyLeaveRejectedAsync(Leave leave, string employeeUserId, string rejectionReason)
//            => SaveAsync(employeeUserId, leave.EmployeeId,
//                "Leave Request Rejected",
//                $"Your leave from {leave.StartDate:dd MMM yyyy} to {leave.EndDate:dd MMM yyyy} has been rejected. Reason: {rejectionReason}",
//                NotificationType.LeaveRejected, leave.Id);

//        public Task NotifyLeaveCancelledAsync(Leave leave, string employeeUserId)
//            => SaveAsync(employeeUserId, leave.EmployeeId,
//                "Leave Cancelled",
//                $"Your leave from {leave.StartDate:dd MMM yyyy} to {leave.EndDate:dd MMM yyyy} has been cancelled.",
//                NotificationType.LeaveCancelled, leave.Id);

//        // ── Department Notifications ──────────────────────────────────────────

//        public Task NotifyDepartmentCreatedAsync(string departmentId, string departmentName, string actorUserId, IEnumerable<string> recipientUserIds)
//            => BroadcastAsync(recipientUserIds, "New Department Created",
//                $"Department '{departmentName}' has been created.",
//                NotificationType.DepartmentCreated, departmentId, actorUserId);

//        public Task NotifyDepartmentUpdatedAsync(string departmentId, string departmentName, string actorUserId, IEnumerable<string> recipientUserIds)
//            => BroadcastAsync(recipientUserIds, "Department Updated",
//                $"Department '{departmentName}' has been updated.",
//                NotificationType.DepartmentUpdated, departmentId, actorUserId);

//        public Task NotifyDepartmentDeletedAsync(string departmentId, string departmentName, string actorUserId, IEnumerable<string> recipientUserIds)
//            => BroadcastAsync(recipientUserIds, "Department Deleted",
//                $"Department '{departmentName}' has been deleted.",
//                NotificationType.DepartmentDeleted, departmentId, actorUserId);

//        public Task NotifyDepartmentStatusChangedAsync(string departmentId, string departmentName, bool isActive, string actorUserId, IEnumerable<string> recipientUserIds)
//            => BroadcastAsync(recipientUserIds,
//                $"Department {(isActive ? "Activated" : "Deactivated")}",
//                $"Department '{departmentName}' has been {(isActive ? "activated" : "deactivated")}.",
//                NotificationType.DepartmentStatusChanged, departmentId, actorUserId);

//        // ── Designation Notifications ─────────────────────────────────────────

//        public Task NotifyDesignationCreatedAsync(string designationId, string designationName, string actorUserId, IEnumerable<string> recipientUserIds)
//            => BroadcastAsync(recipientUserIds, "New Designation Created",
//                $"Designation '{designationName}' has been created.",
//                NotificationType.DesignationCreated, designationId, actorUserId);

//        public Task NotifyDesignationUpdatedAsync(string designationId, string designationName, string actorUserId, IEnumerable<string> recipientUserIds)
//            => BroadcastAsync(recipientUserIds, "Designation Updated",
//                $"Designation '{designationName}' has been updated.",
//                NotificationType.DesignationUpdated, designationId, actorUserId);

//        public Task NotifyDesignationDeletedAsync(string designationId, string designationName, string actorUserId, IEnumerable<string> recipientUserIds)
//            => BroadcastAsync(recipientUserIds, "Designation Deleted",
//                $"Designation '{designationName}' has been deleted.",
//                NotificationType.DesignationDeleted, designationId, actorUserId);

//        public Task NotifyDesignationStatusChangedAsync(string designationId, string designationName, bool isActive, string actorUserId, IEnumerable<string> recipientUserIds)
//            => BroadcastAsync(recipientUserIds,
//                $"Designation {(isActive ? "Activated" : "Deactivated")}",
//                $"Designation '{designationName}' has been {(isActive ? "activated" : "deactivated")}.",
//                NotificationType.DesignationStatusChanged, designationId, actorUserId);

//        // ── Background / Scheduled Jobs ───────────────────────────────────────

//        public async Task SendOverdueAlertsAsync()
//        {
//            try
//            {
//                var today = DateTime.UtcNow.Date;
//                var overdueStatuses = new[] { LeaveStatus.Pending, LeaveStatus.AdminApproved, LeaveStatus.NayabApproved };

//                foreach (var status in overdueStatuses)
//                {
//                    var leaves = await _leaveRepository.GetLeavesByStatusAsync(status);

//                    foreach (var leave in leaves.Where(l => l.StartDate.Date <= today))
//                    {
//                        var employee = await _employeeRepository.GetByIdAsync(leave.EmployeeId);
//                        if (employee == null || string.IsNullOrEmpty(employee.UserId)) continue;

//                        await SaveAsync(employee.UserId, employee.Id,
//                            "Leave Approval Overdue",
//                            $"Your leave request from {leave.StartDate:dd MMM yyyy} to {leave.EndDate:dd MMM yyyy} is still awaiting approval ({status}). Please follow up.",
//                            NotificationType.LeaveApplied, leave.Id);

//                        _logger.LogInformation("Overdue alert sent for leave {LeaveId} (status: {Status})", leave.Id, status);
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error in SendOverdueAlertsAsync");
//            }
//        }

//        public async Task SendWeeklySummaryAsync()
//        {
//            try
//            {
//                var year = DateTime.UtcNow.Year;
//                var allEmployees = await _employeeRepository.GetAllAsync();

//                foreach (var employee in allEmployees)
//                {
//                    if (string.IsNullOrEmpty(employee.UserId)) continue;

//                    var allLeaves = await _leaveRepository.GetLeavesByEmployeeIdAsync(employee.Id);
//                    var pendingCount = allLeaves.Count(l =>
//                        l.LeaveStatus == LeaveStatus.Pending ||
//                        l.LeaveStatus == LeaveStatus.AdminApproved ||
//                        l.LeaveStatus == LeaveStatus.NayabApproved);

//                    var balances = await _leaveBalanceRepository.GetByEmployeeIdAsync(employee.Id, year);
//                    var totalRemaining = balances.Sum(b => b.Available);

//                    var message = pendingCount > 0
//                        ? $"Weekly summary: You have {pendingCount} leave request(s) awaiting approval. Total remaining leave balance: {totalRemaining:0.#} day(s) for {year}."
//                        : $"Weekly summary: No pending leave requests. Total remaining leave balance: {totalRemaining:0.#} day(s) for {year}.";

//                    await SaveAsync(employee.UserId, employee.Id,
//                        "Weekly Leave Summary", message,
//                        NotificationType.LeaveApplied, null);

//                    _logger.LogInformation("Weekly summary sent to employee {EmployeeId}", employee.Id);
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error in SendWeeklySummaryAsync");
//            }
//        }

//        public async Task CheckAndNotifyWorkloadAsync()
//        {
//            try
//            {
//                var today = DateTime.UtcNow.Date;
//                var departments = await _departmentRepository.GetActiveDepartmentsAsync();

//                // ── Resolve Admin user IDs using existing repository methods ──
//                // IRoleRepository.GetByNameAsync exists (used in AdminManagementService)
//                // IUserRepository.GetAllAsync exists (used in AdminManagementService)
//                var adminRole = await _roleRepository.GetByNameAsync("Admin");
//                if (adminRole == null) return;

//                var allUsers = await _userRepository.GetAllAsync();
//                var adminUserIds = allUsers
//                    .Where(u => u.IsActive && u.RoleIds.Contains(adminRole.Id))
//                    .Select(u => u.Id)
//                    .ToList();

//                if (!adminUserIds.Any()) return;

//                foreach (var dept in departments)
//                {
//                    var employees = await _employeeRepository.GetEmployeesByDepartmentAsync(dept.DepartmentId.ToString());
//                    var employeeList = employees.ToList();
//                    if (!employeeList.Any()) continue;

//                    var onLeaveCount = 0;
//                    foreach (var emp in employeeList)
//                    {
//                        var empLeaves = await _leaveRepository.GetLeavesByEmployeeIdAsync(emp.Id);
//                        var isOnLeave = empLeaves.Any(l =>
//                            l.LeaveStatus == LeaveStatus.FullyApproved &&
//                            l.StartDate.Date <= today &&
//                            l.EndDate.Date >= today);

//                        if (isOnLeave) onLeaveCount++;
//                    }

//                    var ratio = (double)onLeaveCount / employeeList.Count;
//                    if (ratio >= WorkloadThresholdPercent)
//                    {
//                        var deptName = dept.DepartmentName ?? dept.DepartmentNameMr;
//                        var pct = (int)(ratio * 100);

//                        await BroadcastAsync(adminUserIds,
//                            "High Leave Workload Alert",
//                            $"Department '{deptName}' has {onLeaveCount} of {employeeList.Count} employees ({pct}%) on approved leave today. Consider workload redistribution.",
//                            NotificationType.DepartmentStatusChanged,
//                            dept.DepartmentId.ToString(),
//                            null);

//                        _logger.LogWarning("Workload alert: {Dept} has {Pct}% on leave today", deptName, pct);
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error in CheckAndNotifyWorkloadAsync");
//            }
//        }

//        // ── Private helpers ───────────────────────────────────────────────────

//        private Task SaveAsync(string recipientUserId, string? recipientEmployeeId,
//            string title, string message, NotificationType type, string? referenceId, string? createdBy = null)
//        {
//            var item = new NotificationItem
//            {
//                RecipientUserId = recipientUserId,
//                RecipientEmployeeId = recipientEmployeeId,
//                Title = title,
//                Message = message,
//                Type = type,
//                ReferenceId = referenceId,
//                CreatedBy = createdBy
//            };
//            return _notificationRepository.CreateAsync(item);
//        }

//        private async Task BroadcastAsync(IEnumerable<string> recipientUserIds,
//            string title, string message, NotificationType type, string? referenceId, string? createdBy)
//        {
//            foreach (var userId in recipientUserIds)
//                await SaveAsync(userId, null, title, message, type, referenceId, createdBy);
//        }

//        private static NotificationDto MapToDto(NotificationItem item) => new()
//        {
//            Id = item.Id,
//            Title = item.Title,
//            Message = item.Message,
//            Type = item.Type.ToString(),
//            ReferenceId = item.ReferenceId,
//            IsRead = item.IsRead,
//            CreatedAt = item.CreatedAt,
//            ReadAt = item.ReadAt
//        };
//    }
//}


using AttendanceManagementSystem.Models.DTOs.Notification;
using AttendanceManagementSystem.Models.Entities;
using AttendanceManagementSystem.Models.Enums;
using AttendanceManagementSystem.Repositories.Interfaces;
using AttendanceManagementSystem.Services.Interfaces;

namespace AttendanceManagementSystem.Services.Implementations
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly ILeaveRepository _leaveRepository;
        private readonly ILeaveBalanceRepository _leaveBalanceRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IDepartmentRepository _departmentRepository;
        private readonly ILogger<NotificationService> _logger;

        private const double WorkloadThresholdPercent = 0.5;

        public NotificationService(
            INotificationRepository notificationRepository,
            ILeaveRepository leaveRepository,
            ILeaveBalanceRepository leaveBalanceRepository,
            IEmployeeRepository employeeRepository,
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IDepartmentRepository departmentRepository,
            ILogger<NotificationService> logger)
        {
            _notificationRepository = notificationRepository;
            _leaveRepository = leaveRepository;
            _leaveBalanceRepository = leaveBalanceRepository;
            _employeeRepository = employeeRepository;
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _departmentRepository = departmentRepository;
            _logger = logger;
        }

        // ── Fetch ─────────────────────────────────────────────────────────────

        public async Task<List<NotificationDto>> GetMyNotificationsAsync(string userId)
        {
            var items = await _notificationRepository.GetByUserIdAsync(userId);
            return items.Select(MapToDto).ToList();
        }

        public async Task<UnreadCountDto> GetMyUnreadCountAsync(string userId)
        {
            var count = await _notificationRepository.GetUnreadCountAsync(userId);
            return new UnreadCountDto { UnreadCount = count };
        }

        // ── Actions ───────────────────────────────────────────────────────────

        public Task<bool> MarkAsReadAsync(string notificationId, string userId)
            => _notificationRepository.MarkAsReadAsync(notificationId, userId);

        public Task<bool> MarkAllAsReadAsync(string userId)
            => _notificationRepository.MarkAllAsReadAsync(userId);

        public Task<bool> DeleteNotificationAsync(string notificationId, string userId)
            => _notificationRepository.DeleteAsync(notificationId, userId);

        // ── Role-Based Leave Notifications ────────────────────────────────────
        //
        // Approval chain:  Employee applies → Admin → NayabTehsildar → Tehsildar
        //
        // Who gets notified at each step:
        //   LeaveApplied          → Admin(s)                 [need to action it]
        //   AdminApproved         → Employee + NayabTehsildar [employee: status update; nayab: next action]
        //   NayabApproved         → Employee + Tehsildar      [employee: status update; tehsildar: next action]
        //   LeaveTehsildarApproved→ Employee                  [fully done]
        //   LeaveRejected         → Employee                  [outcome]
        //   LeaveCancelled        → Employee                  [outcome]

        public async Task NotifyLeaveAppliedAsync(Leave leave, string employeeUserId)
        {
            // 1. Confirm to the employee that their application was received
            await SaveAsync(employeeUserId, leave.EmployeeId,
                "Leave Application Submitted",
                $"Your leave from {leave.StartDate:dd MMM yyyy} to {leave.EndDate:dd MMM yyyy} " +
                $"({leave.TotalDays} day(s)) has been submitted and is pending Admin approval.",
                NotificationType.LeaveApplied, leave.Id);

            // 2. Notify all Admin users — they are the first approver
            var employeeName = await GetEmployeeDisplayNameAsync(leave.EmployeeId);
            var adminIds = await GetUserIdsByRoleAsync("Admin");
            foreach (var adminId in adminIds.Where(id => id != employeeUserId))
            {
                await SaveAsync(adminId, null,
                    "New Leave Application",
                    $"{employeeName} has applied for leave from {leave.StartDate:dd MMM yyyy} " +
                    $"to {leave.EndDate:dd MMM yyyy} ({leave.TotalDays} day(s)). Awaiting your approval.",
                    NotificationType.LeaveApplied, leave.Id, employeeUserId);
            }
        }

        public async Task NotifyLeaveAdminApprovedAsync(Leave leave, string employeeUserId)
        {
            // 1. Notify the employee — status update
            await SaveAsync(employeeUserId, leave.EmployeeId,
                "Leave Approved by Admin",
                $"Your leave from {leave.StartDate:dd MMM yyyy} to {leave.EndDate:dd MMM yyyy} " +
                $"has been approved by the Admin. Awaiting Nayab Tehsildar approval.",
                NotificationType.LeaveAdminApproved, leave.Id);

            // 2. Notify all NayabTehsildar users — they are the next approver
            var employeeName = await GetEmployeeDisplayNameAsync(leave.EmployeeId);
            var nayabIds = await GetUserIdsByRoleAsync("NayabTehsildar");
            foreach (var nayabId in nayabIds)
            {
                await SaveAsync(nayabId, null,
                    "Leave Pending Your Approval",
                    $"{employeeName}'s leave from {leave.StartDate:dd MMM yyyy} to {leave.EndDate:dd MMM yyyy} " +
                    $"has been approved by Admin. Awaiting your (Nayab Tehsildar) approval.",
                    NotificationType.LeaveAdminApproved, leave.Id, leave.AdminApprovedBy);
            }
        }

        public async Task NotifyLeaveNayabApprovedAsync(Leave leave, string employeeUserId)
        {
            // 1. Notify the employee — status update
            await SaveAsync(employeeUserId, leave.EmployeeId,
                "Leave Approved by Nayab Tehsildar",
                $"Your leave from {leave.StartDate:dd MMM yyyy} to {leave.EndDate:dd MMM yyyy} " +
                $"has been approved by the Nayab Tehsildar. Awaiting Tehsildar final approval.",
                NotificationType.LeaveNayabApproved, leave.Id);

            // 2. Notify all Tehsildar users — they are the final approver
            var employeeName = await GetEmployeeDisplayNameAsync(leave.EmployeeId);
            var tehsildarIds = await GetUserIdsByRoleAsync("Tehsildar");
            foreach (var tehsildarId in tehsildarIds)
            {
                await SaveAsync(tehsildarId, null,
                    "Leave Pending Final Approval",
                    $"{employeeName}'s leave from {leave.StartDate:dd MMM yyyy} to {leave.EndDate:dd MMM yyyy} " +
                    $"has been approved by Nayab Tehsildar. Awaiting your (Tehsildar) final approval.",
                    NotificationType.LeaveNayabApproved, leave.Id, leave.NayabApprovedBy);
            }
        }

        public Task NotifyLeaveTehsildarApprovedAsync(Leave leave, string employeeUserId)
            // Only the employee needs to know — approval chain is complete
            => SaveAsync(employeeUserId, leave.EmployeeId,
                "Leave Fully Approved ✓",
                $"Your leave from {leave.StartDate:dd MMM yyyy} to {leave.EndDate:dd MMM yyyy} " +
                $"({leave.TotalDays} day(s)) has been fully approved by the Tehsildar.",
                NotificationType.LeaveTehsildarApproved, leave.Id);

        public Task NotifyLeaveRejectedAsync(Leave leave, string employeeUserId, string rejectionReason)
            => SaveAsync(employeeUserId, leave.EmployeeId,
                "Leave Request Rejected",
                $"Your leave from {leave.StartDate:dd MMM yyyy} to {leave.EndDate:dd MMM yyyy} " +
                $"has been rejected. Reason: {rejectionReason}",
                NotificationType.LeaveRejected, leave.Id);

        public Task NotifyLeaveCancelledAsync(Leave leave, string employeeUserId)
            => SaveAsync(employeeUserId, leave.EmployeeId,
                "Leave Cancelled",
                $"Your leave from {leave.StartDate:dd MMM yyyy} to {leave.EndDate:dd MMM yyyy} has been cancelled.",
                NotificationType.LeaveCancelled, leave.Id);

        // ── Department Notifications ──────────────────────────────────────────

        public Task NotifyDepartmentCreatedAsync(string departmentId, string departmentName, string actorUserId, IEnumerable<string> recipientUserIds)
            => BroadcastAsync(recipientUserIds, "New Department Created",
                $"Department '{departmentName}' has been created.",
                NotificationType.DepartmentCreated, departmentId, actorUserId);

        public Task NotifyDepartmentUpdatedAsync(string departmentId, string departmentName, string actorUserId, IEnumerable<string> recipientUserIds)
            => BroadcastAsync(recipientUserIds, "Department Updated",
                $"Department '{departmentName}' has been updated.",
                NotificationType.DepartmentUpdated, departmentId, actorUserId);

        public Task NotifyDepartmentDeletedAsync(string departmentId, string departmentName, string actorUserId, IEnumerable<string> recipientUserIds)
            => BroadcastAsync(recipientUserIds, "Department Deleted",
                $"Department '{departmentName}' has been deleted.",
                NotificationType.DepartmentDeleted, departmentId, actorUserId);

        public Task NotifyDepartmentStatusChangedAsync(string departmentId, string departmentName, bool isActive, string actorUserId, IEnumerable<string> recipientUserIds)
            => BroadcastAsync(recipientUserIds,
                $"Department {(isActive ? "Activated" : "Deactivated")}",
                $"Department '{departmentName}' has been {(isActive ? "activated" : "deactivated")}.",
                NotificationType.DepartmentStatusChanged, departmentId, actorUserId);

        // ── Designation Notifications ─────────────────────────────────────────

        public Task NotifyDesignationCreatedAsync(string designationId, string designationName, string actorUserId, IEnumerable<string> recipientUserIds)
            => BroadcastAsync(recipientUserIds, "New Designation Created",
                $"Designation '{designationName}' has been created.",
                NotificationType.DesignationCreated, designationId, actorUserId);

        public Task NotifyDesignationUpdatedAsync(string designationId, string designationName, string actorUserId, IEnumerable<string> recipientUserIds)
            => BroadcastAsync(recipientUserIds, "Designation Updated",
                $"Designation '{designationName}' has been updated.",
                NotificationType.DesignationUpdated, designationId, actorUserId);

        public Task NotifyDesignationDeletedAsync(string designationId, string designationName, string actorUserId, IEnumerable<string> recipientUserIds)
            => BroadcastAsync(recipientUserIds, "Designation Deleted",
                $"Designation '{designationName}' has been deleted.",
                NotificationType.DesignationDeleted, designationId, actorUserId);

        public Task NotifyDesignationStatusChangedAsync(string designationId, string designationName, bool isActive, string actorUserId, IEnumerable<string> recipientUserIds)
            => BroadcastAsync(recipientUserIds,
                $"Designation {(isActive ? "Activated" : "Deactivated")}",
                $"Designation '{designationName}' has been {(isActive ? "activated" : "deactivated")}.",
                NotificationType.DesignationStatusChanged, designationId, actorUserId);

        // ── Background / Scheduled Jobs ───────────────────────────────────────

        public async Task SendOverdueAlertsAsync()
        {
            try
            {
                var cutoff = DateTime.UtcNow.AddDays(-3);
                var pending = await _leaveRepository.GetPendingLeavesAsync();

                foreach (var leave in pending.Where(l => l.AppliedDate <= cutoff))
                {
                    var status = leave.LeaveStatus.ToString();
                    var employee = await _employeeRepository.GetByIdAsync(leave.EmployeeId);
                    if (employee == null) continue;

                    var userId = await ResolveUserIdForEmployeeAsync(employee);
                    if (string.IsNullOrEmpty(userId)) continue;

                    await SaveAsync(userId, leave.EmployeeId,
                        "Leave Approval Overdue",
                        $"Your leave request from {leave.StartDate:dd MMM yyyy} to {leave.EndDate:dd MMM yyyy} is still awaiting approval ({status}). Please follow up.",
                        NotificationType.LeaveApplied, leave.Id);

                    _logger.LogInformation("Overdue alert sent for leave {LeaveId} (status: {Status})", leave.Id, status);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SendOverdueAlertsAsync");
            }
        }

        public async Task SendWeeklySummaryAsync()
        {
            try
            {
                var year = DateTime.UtcNow.Year;
                var allEmployees = await _employeeRepository.GetAllAsync();

                foreach (var employee in allEmployees)
                {
                    var userId = await ResolveUserIdForEmployeeAsync(employee);
                    if (string.IsNullOrEmpty(userId)) continue;

                    var allLeaves = await _leaveRepository.GetLeavesByEmployeeIdAsync(employee.Id);
                    var pendingCount = allLeaves.Count(l =>
                        l.LeaveStatus == LeaveStatus.Pending ||
                        l.LeaveStatus == LeaveStatus.AdminApproved ||
                        l.LeaveStatus == LeaveStatus.NayabApproved);

                    var balances = await _leaveBalanceRepository.GetByEmployeeIdAsync(employee.Id, year);
                    var totalRemaining = balances.Sum(b => b.Available);

                    var message = pendingCount > 0
                        ? $"Weekly summary: You have {pendingCount} leave request(s) awaiting approval. Total remaining leave balance: {totalRemaining:0.#} day(s) for {year}."
                        : $"Weekly summary: No pending leave requests. Total remaining leave balance: {totalRemaining:0.#} day(s) for {year}.";

                    await SaveAsync(userId, employee.Id,
                        "Weekly Leave Summary", message,
                        NotificationType.LeaveApplied, null);

                    _logger.LogInformation("Weekly summary sent to employee {EmployeeId}", employee.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SendWeeklySummaryAsync");
            }
        }

        public async Task CheckAndNotifyWorkloadAsync()
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                var departments = await _departmentRepository.GetActiveDepartmentsAsync();

                var adminRole = await _roleRepository.GetByNameAsync("Admin");
                if (adminRole == null) return;

                var allUsers = await _userRepository.GetAllAsync();
                var adminUserIds = allUsers
                    .Where(u => u.IsActive && u.RoleIds.Contains(adminRole.Id))
                    .Select(u => u.Id)
                    .ToList();

                if (!adminUserIds.Any()) return;

                foreach (var dept in departments)
                {
                    var employees = await _employeeRepository.GetEmployeesByDepartmentAsync(dept.DepartmentId.ToString());
                    var employeeList = employees.ToList();
                    if (!employeeList.Any()) continue;

                    var onLeaveCount = 0;
                    foreach (var emp in employeeList)
                    {
                        var empLeaves = await _leaveRepository.GetLeavesByEmployeeIdAsync(emp.Id);
                        var isOnLeave = empLeaves.Any(l =>
                            l.LeaveStatus == LeaveStatus.FullyApproved &&
                            l.StartDate.Date <= today &&
                            l.EndDate.Date >= today);

                        if (isOnLeave) onLeaveCount++;
                    }

                    var ratio = (double)onLeaveCount / employeeList.Count;
                    if (ratio >= WorkloadThresholdPercent)
                    {
                        var deptName = dept.DepartmentName ?? dept.DepartmentNameMr;
                        var pct = (int)(ratio * 100);

                        await BroadcastAsync(adminUserIds,
                            "High Leave Workload Alert",
                            $"Department '{deptName}' has {onLeaveCount} of {employeeList.Count} employees ({pct}%) on approved leave today. Consider workload redistribution.",
                            NotificationType.DepartmentStatusChanged,
                            dept.DepartmentId.ToString(),
                            null);

                        _logger.LogWarning("Workload alert: {Dept} has {Pct}% on leave today", deptName, pct);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CheckAndNotifyWorkloadAsync");
            }
        }

        // ── Private helpers ───────────────────────────────────────────────────

        /// <summary>
        /// Returns all active User IDs that have the given role name.
        /// Uses the same pattern already in CheckAndNotifyWorkloadAsync.
        /// </summary>
        private async Task<List<string>> GetUserIdsByRoleAsync(string roleName)
        {
            try
            {
                var role = await _roleRepository.GetByNameAsync(roleName);
                if (role == null) return new List<string>();

                var allUsers = await _userRepository.GetAllAsync();
                return allUsers
                    .Where(u => u.IsActive && u.RoleIds.Contains(role.Id))
                    .Select(u => u.Id)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resolving user IDs for role {Role}", roleName);
                return new List<string>();
            }
        }

        /// <summary>
        /// Resolves the User.Id for an Employee using a 3-level fallback:
        ///   1. Employee.UserId  (direct link if ever populated)
        ///   2. User.EmployeeId  (reverse link if ever populated)
        ///   3. Email match      (always works — both records share the same email)
        /// </summary>
        private async Task<string?> ResolveUserIdForEmployeeAsync(Employee employee)
        {
            if (!string.IsNullOrEmpty(employee.UserId))
                return employee.UserId;

            var byEmpId = await _userRepository.GetByEmployeeIdAsync(employee.Id);
            if (byEmpId != null) return byEmpId.Id;

            if (!string.IsNullOrEmpty(employee.Email))
            {
                var byEmail = await _userRepository.GetByEmailAsync(employee.Email);
                if (byEmail != null) return byEmail.Id;
            }

            return null;
        }

        /// <summary>Returns a display name for an employee (used in approver-facing messages).</summary>
        private async Task<string> GetEmployeeDisplayNameAsync(string employeeId)
        {
            try
            {
                var emp = await _employeeRepository.GetByIdAsync(employeeId);
                if (emp == null) return "An employee";

                var name = emp.GetFullName("en");
                if (string.IsNullOrWhiteSpace(name)) name = emp.GetFullName("mr");
                if (string.IsNullOrWhiteSpace(name)) name = emp.EmployeeCode ?? "An employee";
                return name;
            }
            catch
            {
                return "An employee";
            }
        }

        private Task SaveAsync(string recipientUserId, string? recipientEmployeeId,
            string title, string message, NotificationType type, string? referenceId, string? createdBy = null)
        {
            var item = new NotificationItem
            {
                RecipientUserId = recipientUserId,
                RecipientEmployeeId = recipientEmployeeId,
                Title = title,
                Message = message,
                Type = type,
                ReferenceId = referenceId,
                CreatedBy = createdBy
            };
            return _notificationRepository.CreateAsync(item);
        }

        private async Task BroadcastAsync(IEnumerable<string> recipientUserIds,
            string title, string message, NotificationType type, string? referenceId, string? createdBy)
        {
            foreach (var userId in recipientUserIds)
                await SaveAsync(userId, null, title, message, type, referenceId, createdBy);
        }

        private static NotificationDto MapToDto(NotificationItem item) => new()
        {
            Id = item.Id,
            Title = item.Title,
            Message = item.Message,
            Type = item.Type.ToString(),
            ReferenceId = item.ReferenceId,
            IsRead = item.IsRead,
            CreatedAt = item.CreatedAt,
            ReadAt = item.ReadAt
        };
    }
}