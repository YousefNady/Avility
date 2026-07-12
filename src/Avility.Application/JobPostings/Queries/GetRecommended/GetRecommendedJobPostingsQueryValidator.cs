using FluentValidation;

namespace Avility.Application.JobPostings.Queries.GetRecommended;

public sealed class GetRecommendedJobPostingsQueryValidator : AbstractValidator<GetRecommendedJobPostingsQuery>
{
    public GetRecommendedJobPostingsQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 50);
    }
}