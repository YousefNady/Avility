using Avility.Domain.Common;
using Avility.Domain.Enums;
using Avility.Domain.Exceptions;

namespace Avility.Domain.Entities;

/// <summary>
/// A single learning/reference resource - just a title, description, and
/// URL. The format behind that URL (article, video, PDF, course) doesn't
/// matter to the domain; Category reflects topic instead, so the same
/// category applies no matter what's actually at the link. Admin-managed;
/// there is no JobSeeker/Company ownership here.
/// </summary>
public sealed class Resource : AuditableEntity
{
    public string Title { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public string Url { get; private set; } = null!;
    public ResourceCategory Category { get; private set; }

    private Resource()
    {
    }

    private Resource(string title, string description, string url, ResourceCategory category)
    {
        Title = title;
        Description = description;
        Url = url;
        Category = category;
    }

    public static Resource Create(string title, string description, string url, ResourceCategory category)
    {
        EnsureValidDetails(title, description, url);
        return new Resource(title.Trim(), description.Trim(), url.Trim(), category);
    }

    public void Update(string title, string description, string url, ResourceCategory category)
    {
        EnsureValidDetails(title, description, url);
        Title = title.Trim();
        Description = description.Trim();
        Url = url.Trim();
        Category = category;
    }

    private static void EnsureValidDetails(string title, string description, string url)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new DomainValidationException("Resource title is required.");
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new DomainValidationException("Resource description is required.");
        }

        if (string.IsNullOrWhiteSpace(url) || !Uri.IsWellFormedUriString(url, UriKind.Absolute))
        {
            throw new DomainValidationException("Resource URL must be a valid absolute URL.");
        }
    }
}