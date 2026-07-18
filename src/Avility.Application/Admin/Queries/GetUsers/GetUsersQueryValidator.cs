using Avility.Application.Common.Constants;
using FluentValidation;

namespace Avility.Application.Admin.Queries.GetUsers;

public sealed class GetUsersQueryValidator : AbstractValidator<GetUsersQuery>
{
    public GetUsersQueryValidator()
    {
        RuleFor(x => x.Role)
            .Must(r => r == Roles.JobSeeker || r == Roles.Company || r == Roles.Admin)
            .When(x => !string.IsNullOrWhiteSpace(x.Role))
            .WithMessage("Role must be one of: JobSeeker, Company, Admin.");
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}