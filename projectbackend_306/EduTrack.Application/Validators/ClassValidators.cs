using EduTrack.Application.DTOs.Classes;
using FluentValidation;

namespace EduTrack.Application.Validators;

public sealed class ClassQueryValidator : AbstractValidator<ClassQueryDto>
{
    public ClassQueryValidator()
    {
        RuleFor(x => x.Search).MaximumLength(150);
        RuleFor(x => x.Semester).MaximumLength(20);
        RuleFor(x => x.CourseId).GreaterThan(0).When(x => x.CourseId.HasValue);
        RuleFor(x => x.TeacherId).GreaterThan(0).When(x => x.TeacherId.HasValue);
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}

public sealed class CreateClassValidator : AbstractValidator<CreateClassDto>
{
    public CreateClassValidator()
    {
        RuleFor(x => x.ClassCode)
            .NotEmpty()
            .MaximumLength(20)
            .Matches("^[A-Za-z0-9._-]+$")
            .WithMessage("Class code may contain only letters, numbers, dots, underscores, and hyphens.");
        RuleFor(x => x.CourseId).GreaterThan(0);
        RuleFor(x => x.TeacherId).GreaterThan(0);
        RuleFor(x => x.Semester).NotEmpty().MaximumLength(20);
        RuleFor(x => x.MaxCapacity).GreaterThan(0);
    }
}

public sealed class UpdateClassValidator : AbstractValidator<UpdateClassDto>
{
    public UpdateClassValidator()
    {
        RuleFor(x => x.ClassCode)
            .NotEmpty()
            .MaximumLength(20)
            .Matches("^[A-Za-z0-9._-]+$")
            .WithMessage("Class code may contain only letters, numbers, dots, underscores, and hyphens.");
        RuleFor(x => x.CourseId).GreaterThan(0);
        RuleFor(x => x.TeacherId).GreaterThan(0);
        RuleFor(x => x.Semester).NotEmpty().MaximumLength(20);
        RuleFor(x => x.MaxCapacity).GreaterThan(0);
    }
}
