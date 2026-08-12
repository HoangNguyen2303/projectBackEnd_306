using EduTrack.Application.DTOs.Departments;
using FluentValidation;

namespace EduTrack.Application.Validators;

public sealed class DepartmentQueryValidator : AbstractValidator<DepartmentQueryDto>
{
    public DepartmentQueryValidator()
    {
        RuleFor(x => x.Search).MaximumLength(120);
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}

public sealed class CreateDepartmentValidator : AbstractValidator<CreateDepartmentDto>
{
    public CreateDepartmentValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(10)
            .Matches("^[A-Za-z0-9_-]+$")
            .WithMessage("Department code may contain only letters, numbers, underscores, and hyphens.");
        RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
    }
}

public sealed class UpdateDepartmentValidator : AbstractValidator<UpdateDepartmentDto>
{
    public UpdateDepartmentValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(10)
            .Matches("^[A-Za-z0-9_-]+$")
            .WithMessage("Department code may contain only letters, numbers, underscores, and hyphens.");
        RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
    }
}
