using AttendanceManagementSystem.Models.DTOs.Designation;
using FluentValidation;

namespace AttendanceManagementSystem.Validators.Designation
{
    public class CreateDesignationValidator : AbstractValidator<CreateDesignationDto>
    {
        public CreateDesignationValidator()
        {
            RuleFor(x => x.DesignationCode)
                .NotEmpty().WithMessage("Designation code is required")
                .MaximumLength(50).WithMessage("Designation code cannot exceed 50 characters")
                .Matches("^[A-Z0-9-_]+$").WithMessage("Designation code can only contain uppercase letters, numbers, hyphens, and underscores");

            RuleFor(x => x.DesignationName)
                .NotEmpty().WithMessage("Designation name is required")
                .MaximumLength(100).WithMessage("Designation name cannot exceed 100 characters");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");

            RuleFor(x => x.Level)
                .GreaterThan(0).WithMessage("Level must be greater than 0")
                .LessThanOrEqualTo(100).WithMessage("Level cannot exceed 100");
        }
    }
}