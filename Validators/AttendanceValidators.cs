using AttendanceManagementSystem.Models.DTOs.Attendance;
using FluentValidation;

namespace AttendanceManagementSystem.Validators.Attendance
{
    public class CheckInValidator : AbstractValidator<CheckInDto>
    {
        public CheckInValidator()
        {
            RuleFor(x => x.EmployeeId)
                .NotEmpty().WithMessage("Employee ID is required");

            RuleFor(x => x.CheckInTime)
                .NotEmpty().WithMessage("Check-in time is required")
             
                .Must(BeValidCheckInTime).WithMessage("Check-in time cannot be in the future");

            RuleFor(x => x.CheckInMethod)
                .IsInEnum().WithMessage("Invalid check-in method");

            RuleFor(x => x.CheckInLocation)
                .SetValidator(new LocationValidator()!)
                .When(x => x.CheckInLocation != null);
        }

        private bool BeValidCheckInTime(DateTime checkInTime)
        {
            
            
            return checkInTime <= DateTime.UtcNow.AddSeconds(60);
        }
    }

    public class CheckOutValidator : AbstractValidator<CheckOutDto>
    {
        public CheckOutValidator()
        {
            RuleFor(x => x.EmployeeId)
                .NotEmpty().WithMessage("Employee ID is required");

            RuleFor(x => x.CheckOutTime)
                .NotEmpty().WithMessage("Check-out time is required")
              
                .Must(BeValidCheckOutTime).WithMessage("Check-out time cannot be in the future");

            RuleFor(x => x.CheckOutMethod)
                .IsInEnum().WithMessage("Invalid check-out method");

            RuleFor(x => x.CheckOutLocation)
                .SetValidator(new LocationValidator()!)
                .When(x => x.CheckOutLocation != null);
        }

        private bool BeValidCheckOutTime(DateTime checkOutTime)
        {
            return checkOutTime <= DateTime.UtcNow.AddSeconds(60);
        }
    }

    public class LocationValidator : AbstractValidator<LocationDto>
    {
        public LocationValidator()
        {
            RuleFor(x => x.Latitude)
                .InclusiveBetween(-90, 90).WithMessage("Latitude must be between -90 and 90");

            RuleFor(x => x.Longitude)
                .InclusiveBetween(-180, 180).WithMessage("Longitude must be between -180 and 180");
        }
    }

    public class ManualAttendanceValidator : AbstractValidator<ManualAttendanceDto>
    {
        public ManualAttendanceValidator()
        {
            RuleFor(x => x.EmployeeId)
                .NotEmpty().WithMessage("Employee ID is required");

            RuleFor(x => x.AttendanceDate)
                .NotEmpty().WithMessage("Attendance date is required")
                .Must(BeValidDate).WithMessage("Attendance date cannot be in the future");

            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("Invalid attendance status");

            RuleFor(x => x.CheckInTime)
                .Must(BeValidTime).WithMessage("Check-in time cannot be in the future")
                .When(x => x.CheckInTime.HasValue);

            RuleFor(x => x.CheckOutTime)
                .Must(BeValidTime).WithMessage("Check-out time cannot be in the future")
                .Must((dto, checkOut) => BeAfterCheckIn(dto, checkOut))
                .WithMessage("Check-out time must be after check-in time")
                .When(x => x.CheckOutTime.HasValue);
        }

        private bool BeValidDate(DateTime date)
        {
            return date.Date <= DateTime.UtcNow.Date;
        }

        private bool BeValidTime(DateTime? time)
        {
           
            return !time.HasValue || time.Value <= DateTime.UtcNow.AddSeconds(60);
        }

        private bool BeAfterCheckIn(ManualAttendanceDto dto, DateTime? checkOutTime)
        {
            if (!checkOutTime.HasValue || !dto.CheckInTime.HasValue)
                return true;

            return checkOutTime.Value > dto.CheckInTime.Value;
        }
    }

    public class AttendanceFilterValidator : AbstractValidator<AttendanceFilterDto>
    {
        public AttendanceFilterValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThan(0).WithMessage("Page number must be greater than 0");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100");

            RuleFor(x => x.StartDate)
                .Must((dto, startDate) => BeValidDateRange(dto, startDate))
                .WithMessage("Start date must be before or equal to end date")
                .When(x => x.StartDate.HasValue && x.EndDate.HasValue);

            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("Invalid attendance status")
                .When(x => x.Status.HasValue);

            RuleFor(x => x.CheckInMethod)
                .IsInEnum().WithMessage("Invalid check-in method")
                .When(x => x.CheckInMethod.HasValue);
        }

        private bool BeValidDateRange(AttendanceFilterDto dto, DateTime? startDate)
        {
            if (!startDate.HasValue || !dto.EndDate.HasValue)
                return true;

            return startDate.Value <= dto.EndDate.Value;
        }
    }
}