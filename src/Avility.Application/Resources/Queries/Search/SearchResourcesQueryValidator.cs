using Avility.Domain.Enums;
using FluentValidation;

namespace Avility.Application.Resources.Queries.Search;

public sealed class SearchResourcesQueryValidator : AbstractValidator<SearchResourcesQuery>
{
    public SearchResourcesQueryValidator()
    {
        RuleFor(x => x.Category)
            .Must(v => Enum.TryParse<ResourceCategory>(v, out _))
            .When(x => !string.IsNullOrWhiteSpace(x.Category))
            .WithMessage("Category must be a valid value.");
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 50);
    }
}