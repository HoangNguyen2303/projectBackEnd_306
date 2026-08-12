using EduTrack.Application.DTOs.Enrollments;
using FluentValidation;

namespace EduTrack.Application.Validators;

public sealed class EnrollmentQueryValidator : AbstractValidator<EnrollmentQueryDto>
{
    public EnrollmentQueryValidator()
    {
        RuleFor(x => x.StudentId).GreaterThan(0).When(x => x.StudentId.HasValue);
        RuleFor(x => x.ClassId).GreaterThan(0).When(x => x.ClassId.HasValue);
        RuleFor(x => x.Semester).MaximumLength(20);
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}

public sealed class CreateEnrollmentValidator : AbstractValidator<CreateEnrollmentDto>
{
    public CreateEnrollmentValidator()
    {
        RuleFor(x => x.StudentId).GreaterThan(0);
        RuleFor(x => x.ClassId).GreaterThan(0);
    }
}
