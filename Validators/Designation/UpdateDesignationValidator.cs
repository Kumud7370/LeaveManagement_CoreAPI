using AttendanceManagementSystem.Models.DTOs.Designation;
using FluentValidation;

namespace AttendanceManagementSystem.Validators.Designation
{
    public class UpdateDesignationValidator : AbstractValidator<UpdateDesignationDto>
    {
        public UpdateDesignationValidator()
        {
            RuleFor(x => x.DesignationCode)
                .MaximumLength(50).WithMessage("Designation code cannot exceed 50 characters")
                .Matches("^[A-Z0-9-_]+$").WithMessage("Designation code can only contain uppercase letters, numbers, hyphens, and underscores")
                .When(x => !string.IsNullOrEmpty(x.DesignationCode));

            RuleFor(x => x.DesignationName)
                .NotEmpty().WithMessage("Designation name cannot be empty")
                .MaximumLength(100).WithMessage("Designation name cannot exceed 100 characters")
                .When(x => !string.IsNullOrEmpty(x.DesignationName));

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters")
                .When(x => x.Description != null);

            RuleFor(x => x.Level)
                .GreaterThan(0).WithMessage("Level must be greater than 0")
                .LessThanOrEqualTo(100).WithMessage("Level cannot exceed 100")
                .When(x => x.Level.HasValue);
        }
    }
}