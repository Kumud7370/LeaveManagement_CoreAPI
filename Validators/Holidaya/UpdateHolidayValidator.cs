using AttendanceManagementSystem.Models.DTOs.Holiday;
using FluentValidation;

namespace AttendanceManagementSystem.Validators.Holiday
{
    public class UpdateHolidayValidator : AbstractValidator<UpdateHolidayDto>
    {
        public UpdateHolidayValidator()
        {
            RuleFor(x => x.HolidayName)
                .MinimumLength(3).WithMessage("Holiday name must be at least 3 characters")
                .MaximumLength(100).WithMessage("Holiday name must not exceed 100 characters")
                .When(x => !string.IsNullOrEmpty(x.HolidayName));

            RuleFor(x => x.HolidayDate)
                .Must(BeValidDate).WithMessage("Holiday date must be a valid date")
                .When(x => x.HolidayDate.HasValue);

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description must not exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.HolidayType)
                .IsInEnum().WithMessage("Invalid holiday type")
                .When(x => x.HolidayType.HasValue);

            RuleFor(x => x.ApplicableDepartments)
                .Must(list => list == null || list.Count > 0).WithMessage("At least one department must be selected")
                .When(x => x.ApplicableDepartments != null);
        }

        private bool BeValidDate(DateTime? date)
        {
            if (!date.HasValue)
                return true;

            return date.Value >= new DateTime(2000, 1, 1) && date.Value <= new DateTime(2100, 12, 31);
        }
    }
}