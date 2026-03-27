using AttendanceManagementSystem.Models.DTOs.Common;
using AttendanceManagementSystem.Models.DTOs.LeaveBalance;
using AttendanceManagementSystem.Models.Entities;
using AttendanceManagementSystem.Repositories.Interfaces;
using AttendanceManagementSystem.Services.Interfaces;

namespace AttendanceManagementSystem.Services.Implementations
{
    public class LeaveBalanceService : ILeaveBalanceService
    {
        private readonly ILeaveBalanceRepository _leaveBalanceRepository;
        private readonly ILeaveTypeRepository _leaveTypeRepository;
        private readonly IEmployeeRepository _employeeRepository;

        public LeaveBalanceService(
            ILeaveBalanceRepository leaveBalanceRepository,
            ILeaveTypeRepository leaveTypeRepository,
            IEmployeeRepository employeeRepository)
        {
            _leaveBalanceRepository = leaveBalanceRepository;
            _leaveTypeRepository = leaveTypeRepository;
            _employeeRepository = employeeRepository;
        }

        public async Task<LeaveBalanceResponseDto?> CreateLeaveBalanceAsync(CreateLeaveBalanceDto dto, string createdBy)
        {
            var employee = await _employeeRepository.GetByIdAsync(dto.EmployeeId);
            if (employee == null)
                return null;

            var leaveType = await _leaveTypeRepository.GetByIdAsync(dto.LeaveTypeId);
            if (leaveType == null || !leaveType.IsActive)
                return null;

            if (await _leaveBalanceRepository.ExistsAsync(dto.EmployeeId, dto.LeaveTypeId, dto.Year))
                return null;

            if (dto.CarriedForward > 0 && leaveType.IsCarryForward)
            {
                if (dto.CarriedForward > leaveType.MaxCarryForwardDays)
                    return null;
            }
            else if (dto.CarriedForward > 0 && !leaveType.IsCarryForward)
            {
                return null;
            }

            var leaveBalance = new LeaveBalance
            {
                EmployeeId = dto.EmployeeId,
                LeaveTypeId = dto.LeaveTypeId,
                Year = dto.Year,
                TotalAllocated = dto.TotalAllocated,
                CarriedForward = dto.CarriedForward,
                Consumed = 0,
                CreatedBy = createdBy
            };

            leaveBalance.UpdateAvailableBalance();

            var createdBalance = await _leaveBalanceRepository.CreateAsync(leaveBalance);
            return await MapToResponseDtoAsync(createdBalance);
        }

        public async Task<LeaveBalanceResponseDto?> GetLeaveBalanceByIdAsync(string id)
        {
            var balance = await _leaveBalanceRepository.GetByIdAsync(id);
            return balance != null ? await MapToResponseDtoAsync(balance) : null;
        }

        public async Task<LeaveBalanceResponseDto?> GetByEmployeeAndLeaveTypeAsync(string employeeId, string leaveTypeId, int year)
        {
            var balance = await _leaveBalanceRepository.GetByEmployeeAndLeaveTypeAsync(employeeId, leaveTypeId, year);
            return balance != null ? await MapToResponseDtoAsync(balance) : null;
        }

        public async Task<PagedResultDto<LeaveBalanceResponseDto>> GetFilteredLeaveBalancesAsync(LeaveBalanceFilterDto filter)
        {
            var (items, totalCount) = await _leaveBalanceRepository.GetFilteredLeaveBalancesAsync(filter);

            var balanceDtos = new List<LeaveBalanceResponseDto>();
            foreach (var balance in items)
            {
                balanceDtos.Add(await MapToResponseDtoAsync(balance));
            }

            return new PagedResultDto<LeaveBalanceResponseDto>(
                balanceDtos,
                totalCount,
                filter.PageNumber,
                filter.PageSize
            );
        }

        public async Task<List<LeaveBalanceResponseDto>> GetByEmployeeIdAsync(string employeeId, int? year = null)
        {
            var balances = await _leaveBalanceRepository.GetByEmployeeIdAsync(employeeId, year);
            var balanceDtos = new List<LeaveBalanceResponseDto>();

            foreach (var balance in balances)
            {
                balanceDtos.Add(await MapToResponseDtoAsync(balance));
            }

            return balanceDtos;
        }

        public async Task<EmployeeLeaveBalanceSummaryDto?> GetEmployeeBalanceSummaryAsync(string employeeId, int year)
        {
            var employee = await _employeeRepository.GetByIdAsync(employeeId);
            if (employee == null)
                return null;

            var balances = await _leaveBalanceRepository.GetByEmployeeIdAsync(employeeId, year);

            var leaveTypeBalances = new List<LeaveTypeBalanceDto>();
            decimal totalAllocated = 0;
            decimal totalConsumed = 0;
            decimal totalAvailable = 0;

            foreach (var balance in balances)
            {
                var leaveType = await _leaveTypeRepository.GetByIdAsync(balance.LeaveTypeId);
                if (leaveType != null)
                {
                    leaveTypeBalances.Add(new LeaveTypeBalanceDto
                    {
                        LeaveTypeId = balance.LeaveTypeId,
                        LeaveTypeName = leaveType.Name,
                        LeaveTypeCode = leaveType.Code,
                        LeaveTypeColor = leaveType.Color,
                        TotalAllocated = balance.TotalAllocated,
                        Consumed = balance.Consumed,
                        CarriedForward = balance.CarriedForward,
                        Available = balance.Available,
                        UtilizationPercentage = balance.GetUtilizationPercentage()
                    });

                    totalAllocated += balance.TotalAllocated + balance.CarriedForward;
                    totalConsumed += balance.Consumed;
                    totalAvailable += balance.Available;
                }
            }

            return new EmployeeLeaveBalanceSummaryDto
            {
                EmployeeId = employeeId,
                EmployeeCode = employee.EmployeeCode,
                EmployeeName = employee.GetFullName(),
                Year = year,
                LeaveBalances = leaveTypeBalances,
                TotalAllocated = totalAllocated,
                TotalConsumed = totalConsumed,
                TotalAvailable = totalAvailable,
                OverallUtilizationPercentage = totalAllocated > 0 ? (totalConsumed / totalAllocated) * 100 : 0
            };
        }

        public async Task<LeaveBalanceResponseDto?> UpdateLeaveBalanceAsync(string id, UpdateLeaveBalanceDto dto, string updatedBy)
        {
            var balance = await _leaveBalanceRepository.GetByIdAsync(id);
            if (balance == null)
                return null;

            if (dto.TotalAllocated.HasValue)
                balance.TotalAllocated = dto.TotalAllocated.Value;

            if (dto.Consumed.HasValue)
                balance.Consumed = dto.Consumed.Value;

            if (dto.CarriedForward.HasValue)
            {
                var leaveType = await _leaveTypeRepository.GetByIdAsync(balance.LeaveTypeId);
                if (leaveType != null && leaveType.IsCarryForward)
                {
                    if (dto.CarriedForward.Value <= leaveType.MaxCarryForwardDays)
                        balance.CarriedForward = dto.CarriedForward.Value;
                    else
                        return null;
                }
            }

            balance.UpdateAvailableBalance();
            balance.UpdatedBy = updatedBy;

            var updated = await _leaveBalanceRepository.UpdateAsync(id, balance);
            return updated ? await MapToResponseDtoAsync(balance) : null;
        }

        public async Task<bool> DeleteLeaveBalanceAsync(string id, string deletedBy)
        {
            var balance = await _leaveBalanceRepository.GetByIdAsync(id);
            if (balance == null)
                return false;

            balance.UpdatedBy = deletedBy;
            balance.DeletedAt = DateTime.UtcNow;

            return await _leaveBalanceRepository.DeleteAsync(id);
        }

        public async Task<bool> AdjustLeaveBalanceAsync(string id, AdjustLeaveBalanceDto dto, string adjustedBy)
        {
            var balance = await _leaveBalanceRepository.GetByIdAsync(id);
            if (balance == null)
                return false;

            balance.TotalAllocated += dto.AdjustmentAmount;

            if (balance.TotalAllocated < balance.Consumed)
                return false;

            balance.UpdateAvailableBalance();
            balance.UpdatedBy = adjustedBy;

            return await _leaveBalanceRepository.UpdateAsync(id, balance);
        }

        public async Task<bool> ConsumeLeaveAsync(string employeeId, string leaveTypeId, int year, decimal days)
        {
            var balance = await _leaveBalanceRepository.GetByEmployeeAndLeaveTypeAsync(employeeId, leaveTypeId, year);
            if (balance == null)
                return false;

            if (!balance.ConsumeLeave(days))
                return false;

            return await _leaveBalanceRepository.UpdateAsync(balance.Id, balance);
        }

        public async Task<bool> RestoreLeaveAsync(string employeeId, string leaveTypeId, int year, decimal days)
        {
            var balance = await _leaveBalanceRepository.GetByEmployeeAndLeaveTypeAsync(employeeId, leaveTypeId, year);
            if (balance == null)
                return false;

            balance.RestoreLeave(days);

            return await _leaveBalanceRepository.UpdateAsync(balance.Id, balance);
        }

        public async Task<bool> CarryForwardLeaveAsync(CarryForwardDto dto, string performedBy)
        {
            var sourceBalance = await _leaveBalanceRepository.GetByEmployeeAndLeaveTypeAsync(
                dto.EmployeeId, dto.LeaveTypeId, dto.FromYear);

            if (sourceBalance == null || sourceBalance.Available < dto.CarryForwardDays)
                return false;

            var leaveType = await _leaveTypeRepository.GetByIdAsync(dto.LeaveTypeId);
            if (leaveType == null || !leaveType.IsCarryForward)
                return false;

            if (dto.CarryForwardDays > leaveType.MaxCarryForwardDays)
                return false;

            var targetBalance = await _leaveBalanceRepository.GetByEmployeeAndLeaveTypeAsync(
                dto.EmployeeId, dto.LeaveTypeId, dto.ToYear);

            if (targetBalance == null)
            {
                var createDto = new CreateLeaveBalanceDto
                {
                    EmployeeId = dto.EmployeeId,
                    LeaveTypeId = dto.LeaveTypeId,
                    Year = dto.ToYear,
                    TotalAllocated = leaveType.MaxDaysPerYear,
                    CarriedForward = dto.CarryForwardDays
                };

                await CreateLeaveBalanceAsync(createDto, performedBy);
            }
            else
            {
                targetBalance.CarriedForward += dto.CarryForwardDays;
                targetBalance.UpdateAvailableBalance();
                targetBalance.UpdatedBy = performedBy;
                await _leaveBalanceRepository.UpdateAsync(targetBalance.Id, targetBalance);
            }

            return true;
        }

        public async Task<List<LeaveBalanceResponseDto>> GetLowBalanceAlertsAsync(decimal threshold = 2)
        {
            var balances = await _leaveBalanceRepository.GetLowBalanceAlerts(threshold);
            var balanceDtos = new List<LeaveBalanceResponseDto>();

            foreach (var balance in balances)
            {
                balanceDtos.Add(await MapToResponseDtoAsync(balance));
            }

            return balanceDtos;
        }

        public async Task<List<LeaveBalanceResponseDto>> GetExpiringSoonAsync(int year, int daysThreshold = 30)
        {
            var balances = await _leaveBalanceRepository.GetExpiringSoonAsync(year, daysThreshold);
            var balanceDtos = new List<LeaveBalanceResponseDto>();

            foreach (var balance in balances)
            {
                balanceDtos.Add(await MapToResponseDtoAsync(balance));
            }

            return balanceDtos;
        }

        public async Task<bool> InitializeBalanceForEmployeeAsync(string employeeId, int year, string createdBy)
        {
            var employee = await _employeeRepository.GetByIdAsync(employeeId);
            if (employee == null)
                return false;

            var activeLeaveTypes = await _leaveTypeRepository.GetActiveLeaveTypesAsync();

            foreach (var leaveType in activeLeaveTypes)
            {
                var exists = await _leaveBalanceRepository.ExistsAsync(employeeId, leaveType.Id, year);
                if (exists)
                    continue;

                decimal carriedForward = 0;
                if (leaveType.IsCarryForward && year > 2000)
                {
                    var previousBalance = await _leaveBalanceRepository.GetByEmployeeAndLeaveTypeAsync(
                        employeeId, leaveType.Id, year - 1);

                    if (previousBalance != null && previousBalance.Available > 0)
                    {
                        carriedForward = Math.Min(previousBalance.Available, leaveType.MaxCarryForwardDays);
                    }
                }

                var createDto = new CreateLeaveBalanceDto
                {
                    EmployeeId = employeeId,
                    LeaveTypeId = leaveType.Id,
                    Year = year,
                    TotalAllocated = leaveType.MaxDaysPerYear,
                    CarriedForward = carriedForward
                };

                await CreateLeaveBalanceAsync(createDto, createdBy);
            }

            return true;
        }

        public async Task<Dictionary<string, int>> BulkInitializeBalancesAsync(BulkInitializeBalanceDto dto, string createdBy)
        {
            var results = new Dictionary<string, int>
            {
                { "Success", 0 },
                { "Failed", 0 },
                { "Skipped", 0 }
            };

            foreach (var employeeId in dto.EmployeeIds)
            {
                var employee = await _employeeRepository.GetByIdAsync(employeeId);
                if (employee == null)
                {
                    results["Failed"]++;
                    continue;
                }

                var success = await InitializeBalanceForEmployeeAsync(employeeId, dto.Year, createdBy);
                if (success)
                    results["Success"]++;
                else
                    results["Failed"]++;
            }

            return results;
        }

        public async Task<bool> RecalculateBalanceAsync(string id)
        {
            var balance = await _leaveBalanceRepository.GetByIdAsync(id);
            if (balance == null)
                return false;

            balance.UpdateAvailableBalance();

            return await _leaveBalanceRepository.UpdateAsync(id, balance);
        }

        public async Task<CollectiveAssignmentResultDto> AssignCollectiveLeaveBalanceAsync(
            CollectiveLeaveBalancedto dto,
            string createdBy)
        {
            var result = new CollectiveAssignmentResultDto();

            // Resolve and validate the leave type once, up front
            var leaveType = await _leaveTypeRepository.GetByIdAsync(dto.LeaveTypeId!);
            if (leaveType == null || !leaveType.IsActive)
            {
                result.Failed = dto.EmployeeIds.Count;
                result.FailedEmployeeIds.AddRange(dto.EmployeeIds);
                return result;
            }

            // Resolve allocation: use caller-supplied value or fall back to the type default
            decimal allocation = dto.TotalAllocated ?? leaveType.MaxDaysPerYear;

            // Validate carry-forward rules once before looping employees
            if (dto.CarriedForward.HasValue && dto.CarriedForward.Value > 0)
            {
                if (!leaveType.IsCarryForward || dto.CarriedForward.Value > leaveType.MaxCarryForwardDays)
                {
                    result.Failed = dto.EmployeeIds.Count;
                    result.FailedEmployeeIds.AddRange(dto.EmployeeIds);
                    return result;
                }
            }

            foreach (var employeeId in dto.EmployeeIds)
            {
                var employee = await _employeeRepository.GetByIdAsync(employeeId);
                if (employee == null)
                {
                    result.Failed++;
                    result.FailedEmployeeIds.Add(employeeId);
                    continue;
                }

                var alreadyExists = await _leaveBalanceRepository.ExistsAsync(
                    employeeId, leaveType.Id, dto.Year);

                if (alreadyExists)
                {
                    if (dto.SkipExisting)
                    {
                        result.Skipped++;
                        result.SkippedEmployeeIds.Add(employeeId);
                        continue;
                    }

                    // Overwrite mode: update TotalAllocated (and CarriedForward if supplied)
                    var existing = await _leaveBalanceRepository
                        .GetByEmployeeAndLeaveTypeAsync(employeeId, leaveType.Id, dto.Year);

                    if (existing == null)
                    {
                        result.Failed++;
                        result.FailedEmployeeIds.Add(employeeId);
                        continue;
                    }

                    existing.TotalAllocated = allocation;

                    if (dto.CarriedForward.HasValue)
                        existing.CarriedForward = dto.CarriedForward.Value;

                    existing.UpdateAvailableBalance();
                    existing.UpdatedBy = createdBy;

                    var updated = await _leaveBalanceRepository.UpdateAsync(existing.Id, existing);
                    if (updated) result.Succeeded++;
                    else { result.Failed++; result.FailedEmployeeIds.Add(employeeId); }

                    continue;
                }

                // New record — auto-calculate carry-forward from previous year when not supplied
                decimal carryForward = 0;
                if (dto.CarriedForward.HasValue)
                {
                    carryForward = dto.CarriedForward.Value;
                }
                else if (leaveType.IsCarryForward && dto.Year > 2000)
                {
                    var previousBalance = await _leaveBalanceRepository
                        .GetByEmployeeAndLeaveTypeAsync(employeeId, leaveType.Id, dto.Year - 1);

                    if (previousBalance != null && previousBalance.Available > 0)
                        carryForward = Math.Min(previousBalance.Available, leaveType.MaxCarryForwardDays);
                }

                var newBalance = new LeaveBalance
                {
                    EmployeeId = employeeId,
                    LeaveTypeId = leaveType.Id,
                    Year = dto.Year,
                    TotalAllocated = allocation,
                    CarriedForward = carryForward,
                    Consumed = 0,
                    CreatedBy = createdBy
                };

                newBalance.UpdateAvailableBalance();

                var created = await _leaveBalanceRepository.CreateAsync(newBalance);
                if (created != null) result.Succeeded++;
                else { result.Failed++; result.FailedEmployeeIds.Add(employeeId); }
            }

            return result;
        }

        private async Task<LeaveBalanceResponseDto> MapToResponseDtoAsync(LeaveBalance balance)
        {
            var employee = await _employeeRepository.GetByIdAsync(balance.EmployeeId);
            var leaveType = await _leaveTypeRepository.GetByIdAsync(balance.LeaveTypeId);

            return new LeaveBalanceResponseDto
            {
                Id = balance.Id,
                EmployeeId = balance.EmployeeId,
                EmployeeCode = employee?.EmployeeCode,
                EmployeeName = new[] {
                employee.GetFullName("mr"),
                employee.GetFullName("en"),
                employee.GetFullName("hi")
                }.FirstOrDefault(n => !string.IsNullOrWhiteSpace(n))
                ?? employee.EmployeeCode,
                LeaveTypeId = balance.LeaveTypeId,
                LeaveTypeName = leaveType?.Name,
                LeaveTypeCode = leaveType?.Code,
                LeaveTypeColor = leaveType?.Color,
                Year = balance.Year,
                TotalAllocated = balance.TotalAllocated,
                Consumed = balance.Consumed,
                CarriedForward = balance.CarriedForward,
                Available = balance.Available,
                UtilizationPercentage = balance.GetUtilizationPercentage(),
                IsLowBalance = balance.Available <= 2,
                IsCurrentYear = balance.IsCurrentYear(),
                LastUpdated = balance.LastUpdated,
                CreatedAt = balance.CreatedAt,
                UpdatedAt = balance.UpdatedAt
            };
        }
    }
}