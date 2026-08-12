using EduTrack.Application.DTOs.Courses;
using FluentValidation;

namespace EduTrack.Application.Validators;

public sealed class CourseQueryValidator : AbstractValidator<CourseQueryDto>
{
    public CourseQueryValidator()
    {
        RuleFor(x => x.Search).MaximumLength(150);
        RuleFor(x => x.DepartmentId).GreaterThan(0).When(x => x.DepartmentId.HasValue);
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}

public sealed class CreateCourseValidator : AbstractValidator<CreateCourseDto>
{
    public CreateCourseValidator()
    {
        RuleFor(x => x.CourseCode)
            .NotEmpty()
            .MaximumLength(20)
            .Matches("^[A-Za-z0-9._-]+$")
            .WithMessage("Course code may contain only letters, numbers, dots, underscores, and hyphens.");
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Credits).InclusiveBetween(1, 10);
        RuleFor(x => x.DepartmentId).GreaterThan(0);
    }
}

public sealed class UpdateCourseValidator : AbstractValidator<UpdateCourseDto>
{
    public UpdateCourseValidator()
    {
        RuleFor(x => x.CourseCode)
            .NotEmpty()
            .MaximumLength(20)
            .Matches("^[A-Za-z0-9._-]+$")
            .WithMessage("Course code may contain only letters, numbers, dots, underscores, and hyphens.");
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Credits).InclusiveBetween(1, 10);
        RuleFor(x => x.DepartmentId).GreaterThan(0);
    }
}
