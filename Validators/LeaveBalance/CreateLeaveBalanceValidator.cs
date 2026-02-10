using AttendanceManagementSystem.Models.DTOs.LeaveBalance;
using FluentValidation;

namespace AttendanceManagementSystem.Validators.LeaveBalance
{
    public class CreateLeaveBalanceValidator : AbstractValidator<CreateLeaveBalanceDto>
    {
        public CreateLeaveBalanceValidator()
        {
            RuleFor(x => x.EmployeeId)
                .NotEmpty().WithMessage("Employee ID is required")
                .Length(24).WithMessage("Employee ID must be 24 characters");

            RuleFor(x => x.LeaveTypeId)
                .NotEmpty().WithMessage("Leave Type ID is required")
                .Length(24).WithMessage("Leave Type ID must be 24 characters");

            RuleFor(x => x.Year)
                .GreaterThanOrEqualTo(2000).WithMessage("Year must be 2000 or later")
                .LessThanOrEqualTo(DateTime.UtcNow.Year + 1).WithMessage("Year cannot be more than 1 year in the future");

            RuleFor(x => x.TotalAllocated)
                .GreaterThan(0).WithMessage("Total allocated must be greater than 0")
                .LessThanOrEqualTo(365).WithMessage("Total allocated cannot exceed 365 days");

            RuleFor(x => x.CarriedForward)
                .GreaterThanOrEqualTo(0).WithMessage("Carried forward must be 0 or greater")
                .LessThanOrEqualTo(x => x.TotalAllocated).WithMessage("Carried forward cannot exceed total allocated");
        }
    }

    public class UpdateLeaveBalanceValidator : AbstractValidator<UpdateLeaveBalanceDto>
    {
        public UpdateLeaveBalanceValidator()
        {
            RuleFor(x => x.TotalAllocated)
                .GreaterThan(0).WithMessage("Total allocated must be greater than 0")
                .LessThanOrEqualTo(365).WithMessage("Total allocated cannot exceed 365 days")
                .When(x => x.TotalAllocated.HasValue);

            RuleFor(x => x.Consumed)
                .GreaterThanOrEqualTo(0).WithMessage("Consumed must be 0 or greater")
                .When(x => x.Consumed.HasValue);

            RuleFor(x => x.CarriedForward)
                .GreaterThanOrEqualTo(0).WithMessage("Carried forward must be 0 or greater")
                .When(x => x.CarriedForward.HasValue);
        }
    }

    public class AdjustLeaveBalanceValidator : AbstractValidator<AdjustLeaveBalanceDto>
    {
        public AdjustLeaveBalanceValidator()
        {
            RuleFor(x => x.AdjustmentAmount)
                .NotEqual(0).WithMessage("Adjustment amount cannot be zero");

            RuleFor(x => x.AdjustmentReason)
                .NotEmpty().WithMessage("Adjustment reason is required")
                .MinimumLength(10).WithMessage("Adjustment reason must be at least 10 characters")
                .MaximumLength(500).WithMessage("Adjustment reason must not exceed 500 characters");

            RuleFor(x => x.AdjustmentType)
                .NotEmpty().WithMessage("Adjustment type is required")
                .MaximumLength(50).WithMessage("Adjustment type must not exceed 50 characters");
        }
    }

    public class CarryForwardValidator : AbstractValidator<CarryForwardDto>
    {
        public CarryForwardValidator()
        {
            RuleFor(x => x.EmployeeId)
                .NotEmpty().WithMessage("Employee ID is required")
                .Length(24).WithMessage("Employee ID must be 24 characters");

            RuleFor(x => x.LeaveTypeId)
                .NotEmpty().WithMessage("Leave Type ID is required")
                .Length(24).WithMessage("Leave Type ID must be 24 characters");

            RuleFor(x => x.FromYear)
                .GreaterThanOrEqualTo(2000).WithMessage("From year must be 2000 or later")
                .LessThan(x => x.ToYear).WithMessage("From year must be less than to year");

            RuleFor(x => x.ToYear)
                .GreaterThan(x => x.FromYear).WithMessage("To year must be greater than from year")
                .LessThanOrEqualTo(DateTime.UtcNow.Year + 1).WithMessage("To year cannot be more than 1 year in the future");

            RuleFor(x => x.CarryForwardDays)
                .GreaterThan(0).WithMessage("Carry forward days must be greater than 0")
                .LessThanOrEqualTo(365).WithMessage("Carry forward days cannot exceed 365");
        }
    }

    public class BulkInitializeBalanceValidator : AbstractValidator<BulkInitializeBalanceDto>
    {
        public BulkInitializeBalanceValidator()
        {
            RuleFor(x => x.EmployeeIds)
                .NotEmpty().WithMessage("At least one employee ID is required")
                .Must(x => x.Count > 0).WithMessage("At least one employee ID is required");

            RuleForEach(x => x.EmployeeIds)
                .NotEmpty().WithMessage("Employee ID cannot be empty")
                .Length(24).WithMessage("Employee ID must be 24 characters");

            RuleFor(x => x.Year)
                .GreaterThanOrEqualTo(2000).WithMessage("Year must be 2000 or later")
                .LessThanOrEqualTo(DateTime.UtcNow.Year + 1).WithMessage("Year cannot be more than 1 year in the future");
        }
    }
}