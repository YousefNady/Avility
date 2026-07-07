using FluentValidation;

namespace Avility.Application.JobApplications.Queries.GetMine;

public sealed class GetMyJobApplicationsQueryValidator : AbstractValidator<GetMyJobApplicationsQuery>
{
    public GetMyJobApplicationsQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 50);
    }
}
