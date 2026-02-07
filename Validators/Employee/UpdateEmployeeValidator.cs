using AttendanceManagementSystem.Models.DTOs.Employee;
using FluentValidation;

namespace AttendanceManagementSystem.Validators.Employee
{
    public class UpdateEmployeeValidator : AbstractValidator<UpdateEmployeeDto>
    {
        public UpdateEmployeeValidator()
        {
            RuleFor(x => x.FirstName)
                .MinimumLength(2).WithMessage("First name must be at least 2 characters")
                .MaximumLength(50).WithMessage("First name must not exceed 50 characters")
                .When(x => !string.IsNullOrEmpty(x.FirstName));

            RuleFor(x => x.MiddleName)
                .MaximumLength(50).WithMessage("Middle name must not exceed 50 characters")
                .When(x => !string.IsNullOrEmpty(x.MiddleName));

            RuleFor(x => x.LastName)
                .MinimumLength(2).WithMessage("Last name must be at least 2 characters")
                .MaximumLength(50).WithMessage("Last name must not exceed 50 characters")
                .When(x => !string.IsNullOrEmpty(x.LastName));

            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("Invalid email format")
                .When(x => !string.IsNullOrEmpty(x.Email));

            RuleFor(x => x.PhoneNumber)
                .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Invalid phone number format")
                .When(x => !string.IsNullOrEmpty(x.PhoneNumber));

            RuleFor(x => x.AlternatePhoneNumber)
                .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Invalid alternate phone number format")
                .When(x => !string.IsNullOrEmpty(x.AlternatePhoneNumber));

            RuleFor(x => x.DateOfBirth)
                .Must(BeAtLeast18YearsOld).WithMessage("Employee must be at least 18 years old")
                .Must(BeValidDate).WithMessage("Invalid date of birth")
                .When(x => x.DateOfBirth.HasValue);

            RuleFor(x => x.Gender)
                .IsInEnum().WithMessage("Invalid gender value")
                .When(x => x.Gender.HasValue);

            RuleFor(x => x.Address)
                .ChildRules(address =>
                {
                    address.RuleFor(a => a!.Street)
                        .NotEmpty().WithMessage("Street is required")
                        .MaximumLength(200).WithMessage("Street must not exceed 200 characters");

                    address.RuleFor(a => a!.City)
                        .NotEmpty().WithMessage("City is required")
                        .MaximumLength(100).WithMessage("City must not exceed 100 characters");

                    address.RuleFor(a => a!.State)
                        .NotEmpty().WithMessage("State is required")
                        .MaximumLength(100).WithMessage("State must not exceed 100 characters");

                    address.RuleFor(a => a!.Country)
                        .NotEmpty().WithMessage("Country is required")
                        .MaximumLength(100).WithMessage("Country must not exceed 100 characters");

                    address.RuleFor(a => a!.PostalCode)
                        .NotEmpty().WithMessage("Postal code is required")
                        .MaximumLength(20).WithMessage("Postal code must not exceed 20 characters");
                })
                .When(x => x.Address != null);

            RuleFor(x => x.DateOfJoining)
                .Must(BeValidDate).WithMessage("Invalid date of joining")
                .When(x => x.DateOfJoining.HasValue);

            RuleFor(x => x.EmploymentType)
                .IsInEnum().WithMessage("Invalid employment type")
                .When(x => x.EmploymentType.HasValue);

            RuleFor(x => x.EmployeeStatus)
                .IsInEnum().WithMessage("Invalid employee status")
                .When(x => x.EmployeeStatus.HasValue);
        }

        private bool BeAtLeast18YearsOld(DateTime? dateOfBirth)
        {
            if (!dateOfBirth.HasValue)
                return true;

            var today = DateTime.UtcNow;
            var age = today.Year - dateOfBirth.Value.Year;
            if (dateOfBirth.Value.Date > today.AddYears(-age)) age--;
            return age >= 18;
        }

        private bool BeValidDate(DateTime? date)
        {
            if (!date.HasValue)
                return true;

            return date.Value >= new DateTime(1900, 1, 1) && date.Value <= DateTime.UtcNow;
        }
    }
}