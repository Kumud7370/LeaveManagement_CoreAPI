using AttendanceManagementSystem.Models.DTOs.Holiday;
using FluentValidation;

namespace AttendanceManagementSystem.Validators.Holiday
{
    public class CreateHolidayValidator : AbstractValidator<CreateHolidayDto>
    {
        public CreateHolidayValidator()
        {
            RuleFor(x => x.HolidayName)
                .NotEmpty().WithMessage("Holiday name is required")
                .MinimumLength(3).WithMessage("Holiday name must be at least 3 characters")
                .MaximumLength(100).WithMessage("Holiday name must not exceed 100 characters");

            RuleFor(x => x.HolidayDate)
                .NotEmpty().WithMessage("Holiday date is required")
                .Must(BeValidDate).WithMessage("Holiday date must be a valid future or current date");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description must not exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.HolidayType)
                .IsInEnum().WithMessage("Invalid holiday type");

            RuleFor(x => x.ApplicableDepartments)
                .NotNull().WithMessage("Applicable departments list is required")
                .Must(list => list == null || list.Count > 0).WithMessage("At least one department must be selected")
                .When(x => x.HolidayType == Models.Enums.HolidayType.Regional || x.HolidayType == Models.Enums.HolidayType.Optional);
        }

        private bool BeValidDate(DateTime date)
        {
            return date >= new DateTime(2000, 1, 1) && date <= new DateTime(2100, 12, 31);
        }
    }
}