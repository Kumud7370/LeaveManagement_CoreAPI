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
            // Check if holiday already exists
            if (await _holidayRepository.IsHolidayExistsAsync(dto.HolidayName, dto.HolidayDate))
                return null;

            var holiday = new Holiday
            {
                HolidayName = dto.HolidayName,
                HolidayDate = dto.HolidayDate,
                Description = dto.Description,
                HolidayType = dto.HolidayType,
                IsOptional = dto.IsOptional,
                ApplicableDepartments = dto.ApplicableDepartments ?? new List<string>(),
                CreatedBy = createdBy
            };

            var createdHoliday = await _holidayRepository.CreateAsync(holiday);
            return MapToResponseDto(createdHoliday);
        }

        public async Task<HolidayResponseDto?> GetHolidayByIdAsync(string id)
        {
            var holiday = await _holidayRepository.GetByIdAsync(id);
            return holiday != null ? MapToResponseDto(holiday) : null;
        }

        public async Task<PagedResultDto<HolidayResponseDto>> GetFilteredHolidaysAsync(HolidayFilterDto filter)
        {
            var (items, totalCount) = await _holidayRepository.GetFilteredHolidaysAsync(filter);

            var holidayDtos = items.Select(MapToResponseDto).ToList();

            return new PagedResultDto<HolidayResponseDto>(
                holidayDtos,
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
            if (holiday == null)
                return null;

            // Check if holiday name/date is being updated and if it already exists
            if (!string.IsNullOrEmpty(dto.HolidayName) && dto.HolidayDate.HasValue)
            {
                if (dto.HolidayName != holiday.HolidayName || dto.HolidayDate.Value.Date != holiday.HolidayDate.Date)
                {
                    if (await _holidayRepository.IsHolidayExistsAsync(
                        dto.HolidayName ?? holiday.HolidayName,
                        dto.HolidayDate ?? holiday.HolidayDate,
                        id))
                        return null;
                }
            }

            // Update only provided fields
            if (!string.IsNullOrEmpty(dto.HolidayName))
                holiday.HolidayName = dto.HolidayName;

            if (dto.HolidayDate.HasValue)
                holiday.HolidayDate = dto.HolidayDate.Value;

            if (dto.Description != null)
                holiday.Description = dto.Description;

            if (dto.HolidayType.HasValue)
                holiday.HolidayType = dto.HolidayType.Value;

            if (dto.IsOptional.HasValue)
                holiday.IsOptional = dto.IsOptional.Value;

            if (dto.ApplicableDepartments != null)
                holiday.ApplicableDepartments = dto.ApplicableDepartments;

            holiday.UpdatedBy = updatedBy;

            var updated = await _holidayRepository.UpdateAsync(id, holiday);
            return updated ? MapToResponseDto(holiday) : null;
        }

        public async Task<bool> DeleteHolidayAsync(string id, string deletedBy)
        {
            var holiday = await _holidayRepository.GetByIdAsync(id);
            if (holiday == null)
                return false;

            holiday.UpdatedBy = deletedBy;
            holiday.DeletedAt = DateTime.UtcNow;

            return await _holidayRepository.DeleteAsync(id);
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
                var count = await _holidayRepository.GetHolidayCountByTypeAsync(type);
                statistics[type.ToString()] = count;
            }

            return statistics;
        }

        private HolidayResponseDto MapToResponseDto(Holiday holiday)
        {
            return new HolidayResponseDto
            {
                Id = holiday.Id,
                HolidayName = holiday.HolidayName,
                HolidayDate = holiday.HolidayDate,
                Description = holiday.Description,
                HolidayType = holiday.HolidayType,
                HolidayTypeName = holiday.HolidayType.ToString(),
                IsOptional = holiday.IsOptional,
                ApplicableDepartments = holiday.ApplicableDepartments,
                DepartmentNames = new List<string>(), // Can be populated if needed
                IsUpcoming = holiday.IsUpcoming(),
                IsToday = holiday.IsToday(),
                DaysUntilHoliday = holiday.DaysUntilHoliday(),
                CreatedAt = holiday.CreatedAt,
                UpdatedAt = holiday.UpdatedAt
            };
        }
    }
}