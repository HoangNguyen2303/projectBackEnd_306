using FluentValidation.Results;

namespace EduTrack.Application.Common;

internal static class ValidationResultExtensions
{
    public static string ToErrorMessage(this ValidationResult result)
        => string.Join(" ", result.Errors
            .Select(error => error.ErrorMessage)
            .Distinct(StringComparer.Ordinal));
}
