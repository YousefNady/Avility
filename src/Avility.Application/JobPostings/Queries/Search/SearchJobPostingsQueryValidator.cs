using Avility.Domain.Enums;
using FluentValidation;

namespace Avility.Application.JobPostings.Queries.Search;

public sealed class SearchJobPostingsQueryValidator : AbstractValidator<SearchJobPostingsQuery>
{
    public SearchJobPostingsQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 50);
        RuleFor(x => x.DisabilityCategory)
            .Must(v => Enum.TryParse<DisabilityCategory>(v, out _))
            .When(x => !string.IsNullOrWhiteSpace(x.DisabilityCategory))
            .WithMessage("DisabilityCategory must be a valid value.");
    }
}
