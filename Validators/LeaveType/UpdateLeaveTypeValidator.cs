using AttendanceManagementSystem.Models.DTOs.LeaveType;
using FluentValidation;

namespace AttendanceManagementSystem.Validators.LeaveType
{
    public class UpdateLeaveTypeValidator : AbstractValidator<UpdateLeaveTypeDto>
    {
        public UpdateLeaveTypeValidator()
        {
            RuleFor(x => x.Name)
                .MinimumLength(2).WithMessage("Name must be at least 2 characters")
                .MaximumLength(100).WithMessage("Name must not exceed 100 characters")
                .When(x => !string.IsNullOrEmpty(x.Name));

            RuleFor(x => x.Code)
                .MinimumLength(2).WithMessage("Code must be at least 2 characters")
                .MaximumLength(10).WithMessage("Code must not exceed 10 characters")
                .Matches("^[A-Z0-9_]+$").WithMessage("Code must contain only uppercase letters, numbers, and underscores")
                .When(x => !string.IsNullOrEmpty(x.Code));

            RuleFor(x => x.Description)
                .MinimumLength(10).WithMessage("Description must be at least 10 characters")
                .MaximumLength(500).WithMessage("Description must not exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.MaxDaysPerYear)
                .GreaterThan(0).WithMessage("Max days per year must be greater than 0")
                .LessThanOrEqualTo(365).WithMessage("Max days per year cannot exceed 365")
                .When(x => x.MaxDaysPerYear.HasValue);

            RuleFor(x => x.MaxCarryForwardDays)
                .GreaterThanOrEqualTo(0).WithMessage("Max carry forward days must be 0 or greater")
                .When(x => x.MaxCarryForwardDays.HasValue);

            RuleFor(x => x.MinimumNoticeDays)
                .GreaterThanOrEqualTo(0).WithMessage("Minimum notice days must be 0 or greater")
                .LessThanOrEqualTo(90).WithMessage("Minimum notice days cannot exceed 90")
                .When(x => x.MinimumNoticeDays.HasValue);

            RuleFor(x => x.Color)
                .Matches("^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$").WithMessage("Color must be a valid hex color code (e.g., #FF5733)")
                .When(x => !string.IsNullOrEmpty(x.Color));

            RuleFor(x => x.DisplayOrder)
                .GreaterThanOrEqualTo(0).WithMessage("Display order must be 0 or greater")
                .When(x => x.DisplayOrder.HasValue);
        }
    }
}