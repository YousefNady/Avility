using FluentValidation;

namespace Avility.Application.JobPostings.Queries.GetMine;

public sealed class GetMyJobPostingsQueryValidator : AbstractValidator<GetMyJobPostingsQuery>
{
    public GetMyJobPostingsQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 50);
    }
}
