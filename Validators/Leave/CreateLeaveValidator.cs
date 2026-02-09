using AttendanceManagementSystem.Models.DTOs.Leave;
using FluentValidation;

namespace AttendanceManagementSystem.Validators.Leave
{
    public class CreateLeaveValidator : AbstractValidator<CreateLeaveDto>
    {
        public CreateLeaveValidator()
        {
            RuleFor(x => x.EmployeeId)
                .NotEmpty().WithMessage("Employee ID is required")
                .Length(24).WithMessage("Employee ID must be 24 characters");

            RuleFor(x => x.LeaveTypeId)
                .NotEmpty().WithMessage("Leave Type ID is required")
                .Length(24).WithMessage("Leave Type ID must be 24 characters");

            RuleFor(x => x.StartDate)
                .NotEmpty().WithMessage("Start date is required")
                .Must(BeValidDate).WithMessage("Invalid start date");

            RuleFor(x => x.EndDate)
                .NotEmpty().WithMessage("End date is required")
                .Must(BeValidDate).WithMessage("Invalid end date")
                .GreaterThanOrEqualTo(x => x.StartDate).WithMessage("End date must be greater than or equal to start date");

            RuleFor(x => x.TotalDays)
                .GreaterThan(0).WithMessage("Total days must be greater than 0")
                .LessThanOrEqualTo(365).WithMessage("Total days cannot exceed 365");

            RuleFor(x => x.Reason)
                .NotEmpty().WithMessage("Reason is required")
                .MinimumLength(10).WithMessage("Reason must be at least 10 characters")
                .MaximumLength(500).WithMessage("Reason must not exceed 500 characters");

            RuleFor(x => x.AttachmentUrl)
                .MaximumLength(500).WithMessage("Attachment URL must not exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.AttachmentUrl));
        }

        private bool BeValidDate(DateTime date)
        {
            return date >= new DateTime(2000, 1, 1) && date <= DateTime.UtcNow.AddYears(2);
        }
    }
}