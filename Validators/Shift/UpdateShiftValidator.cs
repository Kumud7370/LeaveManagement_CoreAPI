using AttendanceManagementSystem.Models.DTOs.Shift;
using FluentValidation;

namespace AttendanceManagementSystem.Validators.Shift
{
    public class UpdateShiftValidator : AbstractValidator<UpdateShiftDto>
    {
        public UpdateShiftValidator()
        {
            RuleFor(x => x.ShiftName)
                .MinimumLength(2).WithMessage("Shift name must be at least 2 characters")
                .MaximumLength(100).WithMessage("Shift name must not exceed 100 characters")
                .When(x => !string.IsNullOrEmpty(x.ShiftName));

            RuleFor(x => x.ShiftCode)
                .MinimumLength(2).WithMessage("Shift code must be at least 2 characters")
                .MaximumLength(20).WithMessage("Shift code must not exceed 20 characters")
                .Matches("^[A-Z0-9_-]+$").WithMessage("Shift code must contain only uppercase letters, numbers, underscores, and hyphens")
                .When(x => !string.IsNullOrEmpty(x.ShiftCode));

            RuleFor(x => x)
                .Must(x => !x.StartTime.HasValue || !x.EndTime.HasValue || x.StartTime.Value != x.EndTime.Value)
                .WithMessage("Start time and end time cannot be the same")
                .When(x => x.StartTime.HasValue && x.EndTime.HasValue);

            RuleFor(x => x.GracePeriodMinutes)
                .GreaterThanOrEqualTo(0).WithMessage("Grace period must be 0 or greater")
                .LessThanOrEqualTo(60).WithMessage("Grace period cannot exceed 60 minutes")
                .When(x => x.GracePeriodMinutes.HasValue);

            RuleFor(x => x.MinimumWorkingMinutes)
                .GreaterThan(0).WithMessage("Minimum working minutes must be greater than 0")
                .LessThanOrEqualTo(1440).WithMessage("Minimum working minutes cannot exceed 1440 (24 hours)")
                .When(x => x.MinimumWorkingMinutes.HasValue);

            RuleFor(x => x.BreakDurationMinutes)
                .GreaterThanOrEqualTo(0).WithMessage("Break duration must be 0 or greater")
                .LessThanOrEqualTo(240).WithMessage("Break duration cannot exceed 240 minutes (4 hours)")
                .When(x => x.BreakDurationMinutes.HasValue);

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description must not exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.Color)
                .Matches("^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$").WithMessage("Color must be a valid hex color code (e.g., #FF5733 or #F57)")
                .When(x => !string.IsNullOrEmpty(x.Color));

            RuleFor(x => x.NightShiftAllowancePercentage)
                .GreaterThanOrEqualTo(0).WithMessage("Night shift allowance percentage must be 0 or greater")
                .LessThanOrEqualTo(100).WithMessage("Night shift allowance percentage cannot exceed 100")
                .When(x => x.NightShiftAllowancePercentage.HasValue);

            RuleFor(x => x.DisplayOrder)
                .GreaterThanOrEqualTo(0).WithMessage("Display order must be 0 or greater")
                .When(x => x.DisplayOrder.HasValue);
        }
    }
}