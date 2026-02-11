using AttendanceManagementSystem.Models.DTOs.Department;
using FluentValidation;

namespace AttendanceManagementSystem.Validators
{
    public class CreateDepartmentValidator : AbstractValidator<CreateDepartmentRequestDto>
    {
        public CreateDepartmentValidator()
        {
            RuleFor(x => x.DepartmentCode)
                .NotEmpty().WithMessage("Department code is required")
                .Length(2, 20).WithMessage("Department code must be between 2 and 20 characters")
                .Matches("^[A-Z0-9_-]+$").WithMessage("Department code must contain only uppercase letters, numbers, underscores, and hyphens");

            RuleFor(x => x.DepartmentName)
                .NotEmpty().WithMessage("Department name is required")
                .Length(2, 100).WithMessage("Department name must be between 2 and 100 characters")
                .Must(BeValidDepartmentName).WithMessage("Department name contains invalid characters");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.DisplayOrder)
                .GreaterThanOrEqualTo(0).WithMessage("Display order must be a positive number");

            RuleFor(x => x.ParentDepartmentId)
                .NotEqual(Guid.Empty).WithMessage("Parent department ID cannot be empty")
                .When(x => x.ParentDepartmentId.HasValue);

            RuleFor(x => x.HeadOfDepartment)
                .NotEqual(Guid.Empty).WithMessage("Head of department ID cannot be empty")
                .When(x => x.HeadOfDepartment.HasValue);
        }

        private bool BeValidDepartmentName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            return System.Text.RegularExpressions.Regex.IsMatch(name, @"^[a-zA-Z0-9\s&\-\(\)\.]+$");
        }
    }

    public class UpdateDepartmentValidator : AbstractValidator<UpdateDepartmentRequestDto>
    {
        public UpdateDepartmentValidator()
        {
            RuleFor(x => x.DepartmentId)
                .NotEmpty().WithMessage("Department ID is required")
                .NotEqual(Guid.Empty).WithMessage("Department ID cannot be empty");

            RuleFor(x => x.DepartmentCode)
                .NotEmpty().WithMessage("Department code is required")
                .Length(2, 20).WithMessage("Department code must be between 2 and 20 characters")
                .Matches("^[A-Z0-9_-]+$").WithMessage("Department code must contain only uppercase letters, numbers, underscores, and hyphens");

            RuleFor(x => x.DepartmentName)
                .NotEmpty().WithMessage("Department name is required")
                .Length(2, 100).WithMessage("Department name must be between 2 and 100 characters")
                .Must(BeValidDepartmentName).WithMessage("Department name contains invalid characters");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.DisplayOrder)
                .GreaterThanOrEqualTo(0).WithMessage("Display order must be a positive number");

            RuleFor(x => x.ParentDepartmentId)
                .NotEqual(Guid.Empty).WithMessage("Parent department ID cannot be empty")
                .NotEqual(x => x.DepartmentId).WithMessage("Department cannot be its own parent")
                .When(x => x.ParentDepartmentId.HasValue);

            RuleFor(x => x.HeadOfDepartment)
                .NotEqual(Guid.Empty).WithMessage("Head of department ID cannot be empty")
                .When(x => x.HeadOfDepartment.HasValue);
        }

        private bool BeValidDepartmentName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            return System.Text.RegularExpressions.Regex.IsMatch(name, @"^[a-zA-Z0-9\s&\-\(\)\.]+$");
        }
    }
    public class DepartmentFilterValidator : AbstractValidator<DepartmentFilterRequestDto>
    {
        public DepartmentFilterValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThan(0).WithMessage("Page number must be greater than 0");

            RuleFor(x => x.PageSize)
                .GreaterThan(0).WithMessage("Page size must be greater than 0")
                .LessThanOrEqualTo(100).WithMessage("Page size cannot exceed 100");

            RuleFor(x => x.SortBy)
                .Must(BeValidSortField).WithMessage("Invalid sort field")
                .When(x => !string.IsNullOrEmpty(x.SortBy));

            RuleFor(x => x.SortDirection)
                .Must(x => x.ToLower() == "asc" || x.ToLower() == "desc")
                .WithMessage("Sort direction must be 'asc' or 'desc'")
                .When(x => !string.IsNullOrEmpty(x.SortDirection));

            RuleFor(x => x.SearchTerm)
                .MaximumLength(100).WithMessage("Search term cannot exceed 100 characters")
                .When(x => !string.IsNullOrEmpty(x.SearchTerm));

            RuleFor(x => x.ParentDepartmentId)
                .NotEqual(Guid.Empty).WithMessage("Parent department ID cannot be empty")
                .When(x => x.ParentDepartmentId.HasValue);

            RuleFor(x => x.HeadOfDepartment)
                .NotEqual(Guid.Empty).WithMessage("Head of department ID cannot be empty")
                .When(x => x.HeadOfDepartment.HasValue);
        }

        private bool BeValidSortField(string sortBy)
        {
            var validFields = new[] { "DepartmentCode", "DepartmentName", "DisplayOrder", "CreatedAt", "UpdatedAt" };
            return validFields.Contains(sortBy, StringComparer.OrdinalIgnoreCase);
        }
    }
}
