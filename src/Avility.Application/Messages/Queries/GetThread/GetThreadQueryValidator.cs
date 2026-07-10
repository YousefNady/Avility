using FluentValidation;

namespace Avility.Application.Messages.Queries.GetThread;

public sealed class GetThreadQueryValidator : AbstractValidator<GetThreadQuery>
{
    public GetThreadQueryValidator()
    {
        RuleFor(x => x.JobApplicationId).NotEmpty();
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 50);
    }
}