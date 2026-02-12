using AttendanceManagementSystem.Models.DTOs.Attendance;
using AttendanceManagementSystem.Models.DTOs.Common;
using AttendanceManagementSystem.Models.Entities;
using AttendanceManagementSystem.Models.Enums;
using AttendanceManagementSystem.Repositories.Interfaces;
using AttendanceManagementSystem.Services.Interfaces;

namespace AttendanceManagementSystem.Services.Implementations
{
    public class AttendanceService : IAttendanceService
    {
        private readonly IAttendanceRepository _attendanceRepository;
        private readonly IEmployeeRepository _employeeRepository;

        private readonly TimeSpan _standardCheckInTime = new TimeSpan(9, 0, 0); 
        private readonly TimeSpan _graceperiod = new TimeSpan(0, 15, 0); 
        private readonly TimeSpan _minimumWorkingHours = new TimeSpan(8, 0, 0);
        private readonly TimeSpan _halfDayMinimumHours = new TimeSpan(4, 0, 0); 
        private readonly TimeSpan _standardCheckOutTime = new TimeSpan(18, 0, 0); 

        public AttendanceService(
            IAttendanceRepository attendanceRepository,
            IEmployeeRepository employeeRepository)
        {
            _attendanceRepository = attendanceRepository;
            _employeeRepository = employeeRepository;
        }

        public async Task<AttendanceResponseDto?> CheckInAsync(CheckInDto dto, string createdBy)
        {
            var employee = await _employeeRepository.GetByIdAsync(dto.EmployeeId);
            if (employee == null)
                return null;

            var today = DateTime.UtcNow.Date;
            var existingAttendance = await _attendanceRepository.GetByEmployeeAndDateAsync(dto.EmployeeId, today);

            if (existingAttendance != null && existingAttendance.CheckInTime.HasValue)
                return null; 

            var checkInTime = dto.CheckInTime;
            var checkInTimeOfDay = checkInTime.TimeOfDay;
            var lateThreshold = _standardCheckInTime.Add(_graceperiod);

            var isLate = checkInTimeOfDay > lateThreshold;
            var lateMinutes = isLate ? (int)(checkInTimeOfDay - lateThreshold).TotalMinutes : 0;

            var attendance = new Attendance
            {
                EmployeeId = dto.EmployeeId,
                AttendanceDate = today,
                CheckInTime = checkInTime,
                Status = AttendanceStatus.Present,
                IsLate = isLate,
                LateMinutes = isLate ? lateMinutes : null,
                CheckInLocation = dto.CheckInLocation != null ? new Location(
                    dto.CheckInLocation.Latitude,
                    dto.CheckInLocation.Longitude,
                    dto.CheckInLocation.Address) : null,
                CheckInMethod = dto.CheckInMethod,
                CheckInDeviceId = dto.CheckInDeviceId,
                Remarks = dto.Remarks,
                CreatedBy = createdBy
            };

            var created = await _attendanceRepository.CreateAsync(attendance);
            return await MapToResponseDtoAsync(created);
        }

        public async Task<AttendanceResponseDto?> CheckOutAsync(CheckOutDto dto, string updatedBy)
        {
            var today = DateTime.UtcNow.Date;
            var attendance = await _attendanceRepository.GetByEmployeeAndDateAsync(dto.EmployeeId, today);

            if (attendance == null || !attendance.CheckInTime.HasValue)
                return null; 

            if (attendance.CheckOutTime.HasValue)
                return null; 

            var checkOutTime = dto.CheckOutTime;
            var checkOutTimeOfDay = checkOutTime.TimeOfDay;

            var workingHours = attendance.CalculateWorkingHours();
            attendance.CheckOutTime = checkOutTime;
            attendance.WorkingHours = (checkOutTime - attendance.CheckInTime.Value).TotalHours;

            var isEarlyLeave = checkOutTimeOfDay < _standardCheckOutTime &&
                               attendance.WorkingHours < _minimumWorkingHours.TotalHours;
            var earlyLeaveMinutes = isEarlyLeave ?
                (int)(_standardCheckOutTime - checkOutTimeOfDay).TotalMinutes : 0;

            attendance.IsEarlyLeave = isEarlyLeave;
            attendance.EarlyLeaveMinutes = isEarlyLeave ? earlyLeaveMinutes : null;

            attendance.OvertimeHours = attendance.CalculateOvertimeHours(_minimumWorkingHours.TotalHours);

            if (attendance.WorkingHours >= _minimumWorkingHours.TotalHours)
            {
                attendance.Status = AttendanceStatus.Present;
            }
            else if (attendance.WorkingHours >= _halfDayMinimumHours.TotalHours)
            {
                attendance.Status = AttendanceStatus.HalfDay;
            }
            else
            {
                attendance.Status = AttendanceStatus.Absent;
            }

            attendance.CheckOutLocation = dto.CheckOutLocation != null ? new Location(
                dto.CheckOutLocation.Latitude,
                dto.CheckOutLocation.Longitude,
                dto.CheckOutLocation.Address) : null;
            attendance.CheckOutMethod = dto.CheckOutMethod;
            attendance.CheckOutDeviceId = dto.CheckOutDeviceId;

            if (!string.IsNullOrEmpty(dto.Remarks))
            {
                attendance.Remarks = string.IsNullOrEmpty(attendance.Remarks)
                    ? dto.Remarks
                    : $"{attendance.Remarks}; {dto.Remarks}";
            }

            attendance.UpdatedBy = updatedBy;

            var updated = await _attendanceRepository.UpdateAsync(attendance.Id, attendance);
            return updated ? await MapToResponseDtoAsync(attendance) : null;
        }

        public async Task<AttendanceResponseDto?> MarkManualAttendanceAsync(ManualAttendanceDto dto, string createdBy)
        {
            var employee = await _employeeRepository.GetByIdAsync(dto.EmployeeId);
            if (employee == null)
                return null;

            var existingAttendance = await _attendanceRepository.GetByEmployeeAndDateAsync(
                dto.EmployeeId,
                dto.AttendanceDate.Date);

            if (existingAttendance != null)
                return null; 

            var attendance = new Attendance
            {
                EmployeeId = dto.EmployeeId,
                AttendanceDate = dto.AttendanceDate.Date,
                CheckInTime = dto.CheckInTime,
                CheckOutTime = dto.CheckOutTime,
                Status = dto.Status,
                Remarks = dto.Remarks,
                CheckInMethod = CheckInMethod.Manual,
                CreatedBy = createdBy
            };

            if (dto.CheckInTime.HasValue && dto.CheckOutTime.HasValue)
            {
                attendance.WorkingHours = (dto.CheckOutTime.Value - dto.CheckInTime.Value).TotalHours;
                attendance.OvertimeHours = attendance.CalculateOvertimeHours(_minimumWorkingHours.TotalHours);

                var checkInTimeOfDay = dto.CheckInTime.Value.TimeOfDay;
                var lateThreshold = _standardCheckInTime.Add(_graceperiod);
                attendance.IsLate = checkInTimeOfDay > lateThreshold;
                attendance.LateMinutes = attendance.IsLate ?
                    (int)(checkInTimeOfDay - lateThreshold).TotalMinutes : null;

                var checkOutTimeOfDay = dto.CheckOutTime.Value.TimeOfDay;
                attendance.IsEarlyLeave = checkOutTimeOfDay < _standardCheckOutTime &&
                                         attendance.WorkingHours < _minimumWorkingHours.TotalHours;
                attendance.EarlyLeaveMinutes = attendance.IsEarlyLeave ?
                    (int)(_standardCheckOutTime - checkOutTimeOfDay).TotalMinutes : null;
            }

            var created = await _attendanceRepository.CreateAsync(attendance);
            return await MapToResponseDtoAsync(created);
        }

        public async Task<AttendanceResponseDto?> UpdateAttendanceAsync(string id, ManualAttendanceDto dto, string updatedBy)
        {
            var attendance = await _attendanceRepository.GetByIdAsync(id);
            if (attendance == null)
                return null;

            attendance.CheckInTime = dto.CheckInTime;
            attendance.CheckOutTime = dto.CheckOutTime;
            attendance.Status = dto.Status;
            attendance.Remarks = dto.Remarks;
            attendance.UpdatedBy = updatedBy;

            if (dto.CheckInTime.HasValue && dto.CheckOutTime.HasValue)
            {
                attendance.WorkingHours = (dto.CheckOutTime.Value - dto.CheckInTime.Value).TotalHours;
                attendance.OvertimeHours = attendance.CalculateOvertimeHours(_minimumWorkingHours.TotalHours);
            }

            var updated = await _attendanceRepository.UpdateAsync(id, attendance);
            return updated ? await MapToResponseDtoAsync(attendance) : null;
        }

        public async Task<AttendanceResponseDto?> GetAttendanceByIdAsync(string id)
        {
            var attendance = await _attendanceRepository.GetByIdAsync(id);
            return attendance != null ? await MapToResponseDtoAsync(attendance) : null;
        }

        public async Task<AttendanceResponseDto?> GetTodayAttendanceAsync(string employeeId)
        {
            var today = DateTime.UtcNow.Date;
            var attendance = await _attendanceRepository.GetByEmployeeAndDateAsync(employeeId, today);
            return attendance != null ? await MapToResponseDtoAsync(attendance) : null;
        }

        public async Task<AttendanceResponseDto?> GetAttendanceByDateAsync(string employeeId, DateTime date)
        {
            var attendance = await _attendanceRepository.GetByEmployeeAndDateAsync(employeeId, date.Date);
            return attendance != null ? await MapToResponseDtoAsync(attendance) : null;
        }

        public async Task<PagedResultDto<AttendanceResponseDto>> GetFilteredAttendanceAsync(AttendanceFilterDto filter)
        {
            var (items, totalCount) = await _attendanceRepository.GetFilteredAttendanceAsync(filter);

            var attendanceDtos = new List<AttendanceResponseDto>();
            foreach (var item in items)
            {
                attendanceDtos.Add(await MapToResponseDtoAsync(item));
            }

            return new PagedResultDto<AttendanceResponseDto>(
                attendanceDtos,
                totalCount,
                filter.PageNumber,
                filter.PageSize
            );
        }

        public async Task<List<AttendanceResponseDto>> GetEmployeeAttendanceHistoryAsync(
            string employeeId,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            var attendances = await _attendanceRepository.GetByEmployeeIdAsync(employeeId, startDate, endDate);

            var dtos = new List<AttendanceResponseDto>();
            foreach (var attendance in attendances)
            {
                dtos.Add(await MapToResponseDtoAsync(attendance));
            }

            return dtos;
        }

        public async Task<AttendanceSummaryDto> GetAttendanceSummaryAsync(
            string employeeId,
            DateTime startDate,
            DateTime endDate)
        {
            var employee = await _employeeRepository.GetByIdAsync(employeeId);
            var attendances = await _attendanceRepository.GetByEmployeeIdAsync(employeeId, startDate, endDate);

            var totalDays = (endDate.Date - startDate.Date).Days + 1;
            var presentDays = attendances.Count(a => a.Status == AttendanceStatus.Present);
            var absentDays = attendances.Count(a => a.Status == AttendanceStatus.Absent);
            var halfDays = attendances.Count(a => a.Status == AttendanceStatus.HalfDay);
            var leaveDays = attendances.Count(a => a.Status == AttendanceStatus.Leave);
            var holidays = attendances.Count(a => a.Status == AttendanceStatus.Holiday);
            var weekOffs = attendances.Count(a => a.Status == AttendanceStatus.WeekOff);
            var lateDays = attendances.Count(a => a.IsLate);
            var earlyLeaveDays = attendances.Count(a => a.IsEarlyLeave);

            var totalWorkingHours = attendances
                .Where(a => a.WorkingHours.HasValue)
                .Sum(a => a.WorkingHours.Value);

            var totalOvertimeHours = attendances
                .Where(a => a.OvertimeHours.HasValue)
                .Sum(a => a.OvertimeHours.Value);

            var workingDays = totalDays - holidays - weekOffs;
            var attendancePercentage = workingDays > 0
                ? Math.Round((double)(presentDays + halfDays * 0.5) / workingDays * 100, 2)
                : 0;

            var averageWorkingHours = presentDays > 0
                ? Math.Round(totalWorkingHours / presentDays, 2)
                : 0;

            return new AttendanceSummaryDto
            {
                EmployeeId = employeeId,
                EmployeeCode = employee?.EmployeeCode ?? "",
                EmployeeName = employee?.GetFullName() ?? "",
                StartDate = startDate,
                EndDate = endDate,
                TotalWorkingDays = workingDays,
                PresentDays = presentDays,
                AbsentDays = absentDays,
                HalfDays = halfDays,
                LeaveDays = leaveDays,
                Holidays = holidays,
                WeekOffs = weekOffs,
                LateDays = lateDays,
                EarlyLeaveDays = earlyLeaveDays,
                TotalWorkingHours = Math.Round(totalWorkingHours, 2),
                TotalOvertimeHours = Math.Round(totalOvertimeHours, 2),
                AverageWorkingHours = averageWorkingHours,
                AttendancePercentage = attendancePercentage
            };
        }

        public async Task<Dictionary<string, int>> GetAttendanceStatisticsAsync(DateTime startDate, DateTime endDate)
        {
            return await _attendanceRepository.GetAttendanceStatisticsAsync(startDate, endDate)
                .ContinueWith(t => t.Result.ToDictionary(
                    kvp => kvp.Key.ToString(),
                    kvp => kvp.Value
                ));
        }

        public async Task<List<AttendanceResponseDto>> GetLateCheckInsAsync(DateTime startDate, DateTime endDate)
        {
            var attendances = await _attendanceRepository.GetLateCheckInsAsync(startDate, endDate);

            var dtos = new List<AttendanceResponseDto>();
            foreach (var attendance in attendances)
            {
                dtos.Add(await MapToResponseDtoAsync(attendance));
            }

            return dtos;
        }

        public async Task<List<AttendanceResponseDto>> GetEarlyLeavesAsync(DateTime startDate, DateTime endDate)
        {
            var attendances = await _attendanceRepository.GetEarlyLeavesAsync(startDate, endDate);

            var dtos = new List<AttendanceResponseDto>();
            foreach (var attendance in attendances)
            {
                dtos.Add(await MapToResponseDtoAsync(attendance));
            }

            return dtos;
        }

        public async Task<bool> DeleteAttendanceAsync(string id, string deletedBy)
        {
            var attendance = await _attendanceRepository.GetByIdAsync(id);
            if (attendance == null)
                return false;

            attendance.UpdatedBy = deletedBy;
            attendance.DeletedAt = DateTime.UtcNow;

            return await _attendanceRepository.DeleteAsync(id);
        }

        public async Task<bool> ApproveAttendanceAsync(string id, string approvedBy)
        {
            var attendance = await _attendanceRepository.GetByIdAsync(id);
            if (attendance == null)
                return false;

            attendance.ApprovedBy = approvedBy;
            attendance.ApprovedAt = DateTime.UtcNow;
            attendance.UpdatedBy = approvedBy;

            return await _attendanceRepository.UpdateAsync(id, attendance);
        }

        public async Task MarkAbsentEmployeesAsync(DateTime date)
        {
            var activeEmployees = await _employeeRepository.GetActiveEmployeesAsync();
            var activeEmployeeIds = activeEmployees.Select(e => e.Id).ToList();

            var absentRecords = await _attendanceRepository.GetAbsentEmployeesAsync(date, activeEmployeeIds);

            foreach (var record in absentRecords)
            {
                await _attendanceRepository.CreateAsync(record);
            }
        }

        private async Task<AttendanceResponseDto> MapToResponseDtoAsync(Attendance attendance)
        {
            var employee = await _employeeRepository.GetByIdAsync(attendance.EmployeeId);

            return new AttendanceResponseDto
            {
                Id = attendance.Id,
                EmployeeId = attendance.EmployeeId,
                EmployeeCode = employee?.EmployeeCode ?? "",
                EmployeeName = employee?.GetFullName() ?? "",
                AttendanceDate = attendance.AttendanceDate,
                CheckInTime = attendance.CheckInTime,
                CheckOutTime = attendance.CheckOutTime,
                WorkingHours = attendance.WorkingHours,
                OvertimeHours = attendance.OvertimeHours,
                Status = attendance.Status,
                StatusName = attendance.Status.ToString(),
                IsLate = attendance.IsLate,
                IsEarlyLeave = attendance.IsEarlyLeave,
                LateMinutes = attendance.LateMinutes,
                EarlyLeaveMinutes = attendance.EarlyLeaveMinutes,
                CheckInLocation = attendance.CheckInLocation != null ? new LocationDto
                {
                    Latitude = attendance.CheckInLocation.Latitude,
                    Longitude = attendance.CheckInLocation.Longitude,
                    Address = attendance.CheckInLocation.Address
                } : null,
                CheckOutLocation = attendance.CheckOutLocation != null ? new LocationDto
                {
                    Latitude = attendance.CheckOutLocation.Latitude,
                    Longitude = attendance.CheckOutLocation.Longitude,
                    Address = attendance.CheckOutLocation.Address
                } : null,
                CheckInMethod = attendance.CheckInMethod,
                CheckInMethodName = attendance.CheckInMethod?.ToString(),
                CheckOutMethod = attendance.CheckOutMethod,
                CheckOutMethodName = attendance.CheckOutMethod?.ToString(),
                CheckInDeviceId = attendance.CheckInDeviceId,
                CheckOutDeviceId = attendance.CheckOutDeviceId,
                Remarks = attendance.Remarks,
                ApprovedBy = attendance.ApprovedBy,
                ApprovedAt = attendance.ApprovedAt,
                CreatedAt = attendance.CreatedAt,
                UpdatedAt = attendance.UpdatedAt
            };
        }
    }
}