using FluentValidation;

namespace Avility.Application.JobPostings.Queries.Search;

public sealed class SearchJobPostingsQueryValidator : AbstractValidator<SearchJobPostingsQuery>
{
    public SearchJobPostingsQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 50);
    }
}
