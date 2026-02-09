using AttendanceManagementSystem.Models.DTOs.Leave;
using FluentValidation;

namespace AttendanceManagementSystem.Validators.Leave
{
    public class UpdateLeaveValidator : AbstractValidator<UpdateLeaveDto>
    {
        public UpdateLeaveValidator()
        {
            RuleFor(x => x.StartDate)
                .Must(BeValidDate).WithMessage("Invalid start date")
                .When(x => x.StartDate.HasValue);

            RuleFor(x => x.EndDate)
                .Must(BeValidDate).WithMessage("Invalid end date")
                .When(x => x.EndDate.HasValue);

            RuleFor(x => x)
                .Must(x => !x.EndDate.HasValue || !x.StartDate.HasValue || x.EndDate.Value >= x.StartDate.Value)
                .WithMessage("End date must be greater than or equal to start date")
                .When(x => x.StartDate.HasValue && x.EndDate.HasValue);

            RuleFor(x => x.TotalDays)
                .GreaterThan(0).WithMessage("Total days must be greater than 0")
                .LessThanOrEqualTo(365).WithMessage("Total days cannot exceed 365")
                .When(x => x.TotalDays.HasValue);

            RuleFor(x => x.Reason)
                .MinimumLength(10).WithMessage("Reason must be at least 10 characters")
                .MaximumLength(500).WithMessage("Reason must not exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.Reason));

            RuleFor(x => x.AttachmentUrl)
                .MaximumLength(500).WithMessage("Attachment URL must not exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.AttachmentUrl));
        }

        private bool BeValidDate(DateTime? date)
        {
            if (!date.HasValue)
                return true;

            return date.Value >= new DateTime(2000, 1, 1) && date.Value <= DateTime.UtcNow.AddYears(2);
        }
    }
}