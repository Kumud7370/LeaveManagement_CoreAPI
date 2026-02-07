using AttendanceManagementSystem.Models.DTOs.Employee;
using FluentValidation;

namespace AttendanceManagementSystem.Validators.Employee
{
    public class CreateEmployeeValidator : AbstractValidator<CreateEmployeeDto>
    {
        public CreateEmployeeValidator()
        {
            RuleFor(x => x.EmployeeCode)
                .NotEmpty().WithMessage("Employee code is required")
                .MinimumLength(3).WithMessage("Employee code must be at least 3 characters")
                .MaximumLength(20).WithMessage("Employee code must not exceed 20 characters");

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required")
                .MinimumLength(2).WithMessage("First name must be at least 2 characters")
                .MaximumLength(50).WithMessage("First name must not exceed 50 characters");

            RuleFor(x => x.MiddleName)
                .MaximumLength(50).WithMessage("Middle name must not exceed 50 characters")
                .When(x => !string.IsNullOrEmpty(x.MiddleName));

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required")
                .MinimumLength(2).WithMessage("Last name must be at least 2 characters")
                .MaximumLength(50).WithMessage("Last name must not exceed 50 characters");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required")
                .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Invalid phone number format");

            RuleFor(x => x.AlternatePhoneNumber)
                .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Invalid alternate phone number format")
                .When(x => !string.IsNullOrEmpty(x.AlternatePhoneNumber));

            RuleFor(x => x.DateOfBirth)
                .NotEmpty().WithMessage("Date of birth is required")
                .Must(BeAtLeast18YearsOld).WithMessage("Employee must be at least 18 years old")
                .Must(BeValidDate).WithMessage("Invalid date of birth");

            RuleFor(x => x.Gender)
                .IsInEnum().WithMessage("Invalid gender value");

            RuleFor(x => x.Address)
                .NotNull().WithMessage("Address is required")
                .SetValidator(new AddressValidator());

            RuleFor(x => x.DepartmentId)
                .NotEmpty().WithMessage("Department is required");

            RuleFor(x => x.DesignationId)
                .NotEmpty().WithMessage("Designation is required");

            RuleFor(x => x.DateOfJoining)
                .NotEmpty().WithMessage("Date of joining is required")
                .Must(BeValidDate).WithMessage("Invalid date of joining");

            RuleFor(x => x.DateOfLeaving)
                .Must(BeAfterJoiningDate).WithMessage("Date of leaving must be after date of joining")
                .When(x => x.DateOfLeaving.HasValue);

            RuleFor(x => x.EmploymentType)
                .IsInEnum().WithMessage("Invalid employment type");

            RuleFor(x => x.EmployeeStatus)
                .IsInEnum().WithMessage("Invalid employee status");
        }

        private bool BeAtLeast18YearsOld(DateTime dateOfBirth)
        {
            var today = DateTime.UtcNow;
            var age = today.Year - dateOfBirth.Year;
            if (dateOfBirth.Date > today.AddYears(-age)) age--;
            return age >= 18;
        }

        private bool BeValidDate(DateTime date)
        {
            return date >= new DateTime(1900, 1, 1) && date <= DateTime.UtcNow;
        }

        private bool BeAfterJoiningDate(CreateEmployeeDto dto, DateTime? dateOfLeaving)
        {
            if (!dateOfLeaving.HasValue)
                return true;

            return dateOfLeaving.Value > dto.DateOfJoining;
        }
    }

    public class AddressValidator : AbstractValidator<Models.ValueObjects.Address>
    {
        public AddressValidator()
        {
            RuleFor(x => x.Street)
                .NotEmpty().WithMessage("Street is required")
                .MaximumLength(200).WithMessage("Street must not exceed 200 characters");

            RuleFor(x => x.City)
                .NotEmpty().WithMessage("City is required")
                .MaximumLength(100).WithMessage("City must not exceed 100 characters");

            RuleFor(x => x.State)
                .NotEmpty().WithMessage("State is required")
                .MaximumLength(100).WithMessage("State must not exceed 100 characters");

            RuleFor(x => x.Country)
                .NotEmpty().WithMessage("Country is required")
                .MaximumLength(100).WithMessage("Country must not exceed 100 characters");

            RuleFor(x => x.PostalCode)
                .NotEmpty().WithMessage("Postal code is required")
                .MaximumLength(20).WithMessage("Postal code must not exceed 20 characters");
        }
    }
}