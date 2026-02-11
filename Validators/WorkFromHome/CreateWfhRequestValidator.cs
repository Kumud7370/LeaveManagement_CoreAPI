using AttendanceManagementSystem.Models.DTOs.WorkFromHome;
using AttendanceManagementSystem.Models.Enums;
using FluentValidation;

namespace AttendanceManagementSystem.Validators.WorkFromHome
{
    public class CreateWfhRequestValidator : AbstractValidator<CreateWfhRequestDto>
    {
        public CreateWfhRequestValidator()
        {
            RuleFor(x => x.StartDate)
                .NotEmpty().WithMessage("Start date is required")
                .Must(BeTodayOrFutureDate).WithMessage("Start date must be today or a future date");

            RuleFor(x => x.EndDate)
                .NotEmpty().WithMessage("End date is required")
                .Must(BeTodayOrFutureDate).WithMessage("End date must be today or a future date")
                .GreaterThanOrEqualTo(x => x.StartDate).WithMessage("End date must be greater than or equal to start date");

            RuleFor(x => x.Reason)
                .NotEmpty().WithMessage("Reason is required")
                .MinimumLength(10).WithMessage("Reason must be at least 10 characters")
                .MaximumLength(500).WithMessage("Reason must not exceed 500 characters");

            RuleFor(x => x)
                .Must(NotExceedMaxDuration).WithMessage("WFH request duration cannot exceed 30 days");
        }

        private bool BeTodayOrFutureDate(DateTime date)
        {
            return date.Date >= DateTime.UtcNow.Date;
        }

        private bool NotExceedMaxDuration(CreateWfhRequestDto dto)
        {
            var duration = (dto.EndDate.Date - dto.StartDate.Date).Days + 1;
            return duration <= 30; // Maximum 30 days
        }
    }

    public class UpdateWfhRequestValidator : AbstractValidator<UpdateWfhRequestDto>
    {
        public UpdateWfhRequestValidator()
        {
            RuleFor(x => x.StartDate)
                .Must(BeTodayOrFutureDate).WithMessage("Start date must be today or a future date")
                .When(x => x.StartDate.HasValue);

            RuleFor(x => x.EndDate)
                .Must(BeTodayOrFutureDate).WithMessage("End date must be today or a future date")
                .When(x => x.EndDate.HasValue);

            RuleFor(x => x)
                .Must(EndDateAfterStartDate).WithMessage("End date must be greater than or equal to start date")
                .When(x => x.StartDate.HasValue || x.EndDate.HasValue);

            RuleFor(x => x.Reason)
                .MinimumLength(10).WithMessage("Reason must be at least 10 characters")
                .MaximumLength(500).WithMessage("Reason must not exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.Reason));
        }

        private bool BeTodayOrFutureDate(DateTime? date)
        {
            if (!date.HasValue)
                return true;

            return date.Value.Date >= DateTime.UtcNow.Date;
        }

        private bool EndDateAfterStartDate(UpdateWfhRequestDto dto)
        {
            if (!dto.StartDate.HasValue && !dto.EndDate.HasValue)
                return true;

            // If only one is provided, we can't validate the relationship
            if (!dto.StartDate.HasValue || !dto.EndDate.HasValue)
                return true;

            return dto.EndDate.Value.Date >= dto.StartDate.Value.Date;
        }
    }

    public class ApproveRejectWfhRequestValidator : AbstractValidator<ApproveRejectWfhRequestDto>
    {
        public ApproveRejectWfhRequestValidator()
        {
            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("Invalid status value")
                .Must(BeApprovedOrRejected).WithMessage("Status must be either Approved or Rejected");

            RuleFor(x => x.RejectionReason)
                .NotEmpty().WithMessage("Rejection reason is required when rejecting a request")
                .MinimumLength(10).WithMessage("Rejection reason must be at least 10 characters")
                .MaximumLength(500).WithMessage("Rejection reason must not exceed 500 characters")
                .When(x => x.Status == ApprovalStatus.Rejected);
        }

        private bool BeApprovedOrRejected(ApprovalStatus status)
        {
            return status == ApprovalStatus.Approved || status == ApprovalStatus.Rejected;
        }
    }
}