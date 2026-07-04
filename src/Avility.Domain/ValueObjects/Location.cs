using Avility.Domain.Exceptions;

namespace Avility.Domain.ValueObjects;

/// <summary>
/// Shared by JobSeeker, Company, and JobPosting. Modeled as a record for
/// structural (value-based) equality - two Locations with the same
/// Country/Governorate/City are considered equal, unlike entities which
/// compare by Id. Immutable: there are no setters, only Create.
/// </summary>
public sealed record Location
{
    public string Country { get; }
    public string Governorate { get; }
    public string City { get; }

    private Location(string country, string governorate, string city)
    {
        Country = country;
        Governorate = governorate;
        City = city;
    }

    public static Location Create(string country, string governorate, string city)
    {
        if (string.IsNullOrWhiteSpace(country))
        {
            throw new DomainValidationException("Country is required.");
        }

        if (string.IsNullOrWhiteSpace(governorate))
        {
            throw new DomainValidationException("Governorate is required.");
        }

        if (string.IsNullOrWhiteSpace(city))
        {
            throw new DomainValidationException("City is required.");
        }

        return new Location(country.Trim(), governorate.Trim(), city.Trim());
    }
}
