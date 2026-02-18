using AttendanceManagementSystem.Models.DTOs.Common;
using AttendanceManagementSystem.Models.DTOs.Holiday;
using AttendanceManagementSystem.Models.Entities;
using AttendanceManagementSystem.Models.Enums;
using AttendanceManagementSystem.Repositories.Interfaces;
using AttendanceManagementSystem.Services.Interfaces;

namespace AttendanceManagementSystem.Services.Implementations
{
    public class HolidayService : IHolidayService
    {
        private readonly IHolidayRepository _holidayRepository;

        public HolidayService(IHolidayRepository holidayRepository)
        {
            _holidayRepository = holidayRepository;
        }

        public async Task<HolidayResponseDto?> CreateHolidayAsync(CreateHolidayDto dto, string createdBy)
        {

            if (await _holidayRepository.IsHolidayExistsAsync(dto.HolidayName, dto.HolidayDate))
                return null;

            var holiday = new Holiday
            {
                HolidayName = dto.HolidayName,
                HolidayDate = DateTime.SpecifyKind(dto.HolidayDate.Date, DateTimeKind.Utc),
                Description = dto.Description,
                HolidayType = dto.HolidayType,
                IsOptional = dto.IsOptional,
                ApplicableDepartments = dto.ApplicableDepartments ?? new List<string>(),
                CreatedBy = createdBy
            };

            var created = await _holidayRepository.CreateAsync(holiday);
            return MapToResponseDto(created);
        }

        public async Task<HolidayResponseDto?> GetHolidayByIdAsync(string id)
        {
            var holiday = await _holidayRepository.GetByIdAsync(id);
            return holiday != null && !holiday.IsDeleted ? MapToResponseDto(holiday) : null;
        }

        public async Task<PagedResultDto<HolidayResponseDto>> GetFilteredHolidaysAsync(HolidayFilterDto filter)
        {
            var (items, totalCount) = await _holidayRepository.GetFilteredHolidaysAsync(filter);
            return new PagedResultDto<HolidayResponseDto>(
                items.Select(MapToResponseDto).ToList(),
                totalCount,
                filter.PageNumber,
                filter.PageSize
            );
        }

        public async Task<List<HolidayResponseDto>> GetHolidaysByDepartmentAsync(string departmentId)
        {
            var holidays = await _holidayRepository.GetHolidaysByDepartmentAsync(departmentId);
            return holidays.Select(MapToResponseDto).ToList();
        }

        public async Task<List<HolidayResponseDto>> GetHolidaysByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var holidays = await _holidayRepository.GetHolidaysByDateRangeAsync(startDate, endDate);
            return holidays.Select(MapToResponseDto).ToList();
        }

        public async Task<List<HolidayResponseDto>> GetUpcomingHolidaysAsync(int count = 10)
        {
            var holidays = await _holidayRepository.GetUpcomingHolidaysAsync(count);
            return holidays.Select(MapToResponseDto).ToList();
        }

        public async Task<List<HolidayResponseDto>> GetHolidaysByYearAsync(int year)
        {
            var holidays = await _holidayRepository.GetHolidaysByYearAsync(year);
            return holidays.Select(MapToResponseDto).ToList();
        }

        public async Task<List<HolidayResponseDto>> GetHolidaysByMonthAsync(int year, int month)
        {
            var holidays = await _holidayRepository.GetHolidaysByMonthAsync(year, month);
            return holidays.Select(MapToResponseDto).ToList();
        }

        public async Task<List<HolidayResponseDto>> GetHolidaysByTypeAsync(HolidayType holidayType)
        {
            var holidays = await _holidayRepository.GetHolidaysByTypeAsync(holidayType);
            return holidays.Select(MapToResponseDto).ToList();
        }

        public async Task<HolidayResponseDto?> UpdateHolidayAsync(string id, UpdateHolidayDto dto, string updatedBy)
        {
            var holiday = await _holidayRepository.GetByIdAsync(id);
            if (holiday == null || holiday.IsDeleted)
                return null;

            var targetName = !string.IsNullOrEmpty(dto.HolidayName) ? dto.HolidayName : holiday.HolidayName;
            var targetDate = dto.HolidayDate.HasValue
                ? DateTime.SpecifyKind(dto.HolidayDate.Value.Date, DateTimeKind.Utc)
                : holiday.HolidayDate;

            bool nameChanged = targetName != holiday.HolidayName;
            bool dateChanged = targetDate.Date != holiday.HolidayDate.Date;

            if (nameChanged || dateChanged)
            {
                if (await _holidayRepository.IsHolidayExistsAsync(targetName, targetDate, id))
                    return null;
            }

            if (!string.IsNullOrEmpty(dto.HolidayName))
                holiday.HolidayName = dto.HolidayName;

            if (dto.HolidayDate.HasValue)
                holiday.HolidayDate = DateTime.SpecifyKind(dto.HolidayDate.Value.Date, DateTimeKind.Utc);

            if (dto.Description != null)
                holiday.Description = dto.Description;

            if (dto.HolidayType.HasValue)
                holiday.HolidayType = dto.HolidayType.Value;

            if (dto.IsOptional.HasValue)
                holiday.IsOptional = dto.IsOptional.Value;

            if (dto.ApplicableDepartments != null)
                holiday.ApplicableDepartments = dto.ApplicableDepartments;

            holiday.UpdatedBy = updatedBy;
            holiday.UpdatedAt = DateTime.UtcNow;

            var updated = await _holidayRepository.UpdateAsync(id, holiday);
            return updated ? MapToResponseDto(holiday) : null;
        }

        public async Task<bool> DeleteHolidayAsync(string id, string deletedBy)
        {
            var holiday = await _holidayRepository.GetByIdAsync(id);
            if (holiday == null || holiday.IsDeleted)
                return false;

            return await _holidayRepository.SoftDeleteAsync(id, deletedBy);
        }

        public async Task<bool> IsHolidayOnDateAsync(DateTime date)
        {
            return await _holidayRepository.IsHolidayOnDateAsync(date);
        }

        public async Task<Dictionary<string, int>> GetHolidayStatisticsByTypeAsync()
        {
            var statistics = new Dictionary<string, int>();
            foreach (HolidayType type in Enum.GetValues(typeof(HolidayType)))
            {
                statistics[type.ToString()] = await _holidayRepository.GetHolidayCountByTypeAsync(type);
            }
            return statistics;
        }

        private static HolidayResponseDto MapToResponseDto(Holiday h) => new()
        {
            Id = h.Id,
            HolidayName = h.HolidayName,
            HolidayDate = h.HolidayDate,
            Description = h.Description,
            HolidayType = h.HolidayType,
            HolidayTypeName = h.HolidayType.ToString(),
            IsOptional = h.IsOptional,
            ApplicableDepartments = h.ApplicableDepartments,
            DepartmentNames = new List<string>(),
            IsUpcoming = h.IsUpcoming(),
            IsToday = h.IsToday(),
            DaysUntilHoliday = h.DaysUntilHoliday(),
            CreatedAt = h.CreatedAt,
            UpdatedAt = h.UpdatedAt
        };
    }
}