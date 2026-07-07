using Avility.Domain.Enums;
using FluentValidation;

namespace Avility.Application.Companies.Queries.GetByVerificationStatus;

public sealed class GetCompaniesByVerificationStatusQueryValidator : AbstractValidator<GetCompaniesByVerificationStatusQuery>
{
    public GetCompaniesByVerificationStatusQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 50);
        RuleFor(x => x.Status)
            .Must(v => Enum.TryParse<CompanyVerificationStatus>(v, out _))
            .When(x => !string.IsNullOrWhiteSpace(x.Status))
            .WithMessage("Status must be a valid value.");
    }
}
