using AttendanceManagementSystem.Models.DTOs.LeaveType;
using FluentValidation;

namespace AttendanceManagementSystem.Validators.LeaveType
{
    public class CreateLeaveTypeValidator : AbstractValidator<CreateLeaveTypeDto>
    {
        public CreateLeaveTypeValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required")
                .MinimumLength(2).WithMessage("Name must be at least 2 characters")
                .MaximumLength(100).WithMessage("Name must not exceed 100 characters");

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Code is required")
                .MinimumLength(2).WithMessage("Code must be at least 2 characters")
                .MaximumLength(10).WithMessage("Code must not exceed 10 characters")
                .Matches("^[A-Z0-9_]+$").WithMessage("Code must contain only uppercase letters, numbers, and underscores");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required")
                .MinimumLength(10).WithMessage("Description must be at least 10 characters")
                .MaximumLength(500).WithMessage("Description must not exceed 500 characters");

            RuleFor(x => x.MaxDaysPerYear)
                .GreaterThan(0).WithMessage("Max days per year must be greater than 0")
                .LessThanOrEqualTo(365).WithMessage("Max days per year cannot exceed 365");

            RuleFor(x => x.MaxCarryForwardDays)
                .GreaterThanOrEqualTo(0).WithMessage("Max carry forward days must be 0 or greater")
                .LessThanOrEqualTo(x => x.MaxDaysPerYear).WithMessage("Max carry forward days cannot exceed max days per year")
                .When(x => x.IsCarryForward);

            RuleFor(x => x.MinimumNoticeDays)
                .GreaterThanOrEqualTo(0).WithMessage("Minimum notice days must be 0 or greater")
                .LessThanOrEqualTo(90).WithMessage("Minimum notice days cannot exceed 90");

            RuleFor(x => x.Color)
                .NotEmpty().WithMessage("Color is required")
                .Matches("^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$").WithMessage("Color must be a valid hex color code (e.g., #FF5733)");

            RuleFor(x => x.DisplayOrder)
                .GreaterThanOrEqualTo(0).WithMessage("Display order must be 0 or greater");
        }
    }
}