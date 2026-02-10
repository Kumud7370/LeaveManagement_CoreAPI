using AttendanceManagementSystem.Models.DTOs.AttendanceRegularization;
using FluentValidation;

namespace AttendanceManagementSystem.Validators.AttendanceRegularization
{
    public class RegularizationRequestValidator : AbstractValidator<RegularizationRequestDto>
    {
        public RegularizationRequestValidator()
        {
            RuleFor(x => x.EmployeeId)
                .NotEmpty().WithMessage("Employee ID is required");

            RuleFor(x => x.AttendanceDate)
                .NotEmpty().WithMessage("Attendance date is required")
                .Must(BeValidDate).WithMessage("Attendance date cannot be more than 7 days in the past or in the future");

            RuleFor(x => x.RegularizationType)
                .IsInEnum().WithMessage("Invalid regularization type");

            RuleFor(x => x.Reason)
                .NotEmpty().WithMessage("Reason is required")
                .MaximumLength(500).WithMessage("Reason cannot exceed 500 characters");

            RuleFor(x => x.RequestedCheckIn)
                .Must(BeValidTime).WithMessage("Requested check-in time cannot be in the future")
                .When(x => x.RequestedCheckIn.HasValue);

            RuleFor(x => x.RequestedCheckOut)
                .Must(BeValidTime).WithMessage("Requested check-out time cannot be in the future")
                .Must((dto, checkOut) => BeAfterCheckIn(dto, checkOut))
                .WithMessage("Requested check-out time must be after check-in time")
                .When(x => x.RequestedCheckOut.HasValue);
        }

        private bool BeValidDate(DateTime date)
        {
            var daysDifference = (DateTime.UtcNow.Date - date.Date).Days;
            return daysDifference >= 0 && daysDifference <= 7;
        }

        private bool BeValidTime(DateTime? time)
        {
            return !time.HasValue || time.Value <= DateTime.UtcNow;
        }

        private bool BeAfterCheckIn(RegularizationRequestDto dto, DateTime? checkOutTime)
        {
            if (!checkOutTime.HasValue || !dto.RequestedCheckIn.HasValue)
                return true;

            return checkOutTime.Value > dto.RequestedCheckIn.Value;
        }
    }

    public class RegularizationApprovalValidator : AbstractValidator<RegularizationApprovalDto>
    {
        public RegularizationApprovalValidator()
        {
            RuleFor(x => x.RejectionReason)
                .NotEmpty().WithMessage("Rejection reason is required when rejecting")
                .When(x => !x.IsApproved);

            RuleFor(x => x.RejectionReason)
                .MaximumLength(500).WithMessage("Rejection reason cannot exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.RejectionReason));
        }
    }

    public class RegularizationFilterValidator : AbstractValidator<RegularizationFilterDto>
    {
        public RegularizationFilterValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThan(0).WithMessage("Page number must be greater than 0");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100");

            RuleFor(x => x.StartDate)
                .Must((dto, startDate) => BeValidDateRange(dto, startDate))
                .WithMessage("Start date must be before or equal to end date")
                .When(x => x.StartDate.HasValue && x.EndDate.HasValue);

            RuleFor(x => x.RegularizationType)
                .IsInEnum().WithMessage("Invalid regularization type")
                .When(x => x.RegularizationType.HasValue);

            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("Invalid status")
                .When(x => x.Status.HasValue);
        }

        private bool BeValidDateRange(RegularizationFilterDto dto, DateTime? startDate)
        {
            if (!startDate.HasValue || !dto.EndDate.HasValue)
                return true;

            return startDate.Value <= dto.EndDate.Value;
        }
    }
}