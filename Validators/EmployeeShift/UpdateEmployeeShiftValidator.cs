using AttendanceManagementSystem.Models.DTOs.EmployeeShift;
using FluentValidation;

namespace AttendanceManagementSystem.Validators.EmployeeShift
{
    public class UpdateEmployeeShiftValidator : AbstractValidator<UpdateEmployeeShiftDto>
    {
        public UpdateEmployeeShiftValidator()
        {
            RuleFor(x => x.ShiftId)
                .Length(24).WithMessage("Shift ID must be 24 characters")
                .When(x => !string.IsNullOrEmpty(x.ShiftId));

            RuleFor(x => x.EffectiveFrom)
                .Must(BeValidDate).WithMessage("Effective from date must be a valid date")
                .GreaterThanOrEqualTo(DateTime.UtcNow.Date.AddDays(-7)).WithMessage("Effective from date cannot be more than 7 days in the past")
                .When(x => x.EffectiveFrom.HasValue);

            RuleFor(x => x.EffectiveTo)
                .Must(BeValidDate).WithMessage("Effective to date must be a valid date")
                .When(x => x.EffectiveTo.HasValue);

            RuleFor(x => x)
                .Must(x => !x.EffectiveFrom.HasValue || !x.EffectiveTo.HasValue || x.EffectiveTo.Value >= x.EffectiveFrom.Value)
                .WithMessage("Effective to date must be greater than or equal to effective from date")
                .When(x => x.EffectiveFrom.HasValue && x.EffectiveTo.HasValue);

            RuleFor(x => x)
                .Must(x => !x.EffectiveFrom.HasValue || !x.EffectiveTo.HasValue || (x.EffectiveTo.Value - x.EffectiveFrom.Value).TotalDays <= 365)
                .WithMessage("Shift assignment duration cannot exceed 365 days")
                .When(x => x.EffectiveFrom.HasValue && x.EffectiveTo.HasValue);

            RuleFor(x => x.ChangeReason)
                .MinimumLength(10).WithMessage("Change reason must be at least 10 characters")
                .MaximumLength(500).WithMessage("Change reason must not exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.ChangeReason));
        }

        private bool BeValidDate(DateTime? date)
        {
            if (!date.HasValue)
                return true;

            return date.Value >= new DateTime(2000, 1, 1) && date.Value <= DateTime.UtcNow.AddYears(5);
        }
    }
}