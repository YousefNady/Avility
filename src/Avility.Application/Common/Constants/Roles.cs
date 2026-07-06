namespace Avility.Application.Common.Constants;

/// <summary>
/// Centralizes the three MVP role names as constants, so both the seed
/// data below and any future [Authorize(Roles = ...)] attribute or policy
/// definition in the API layer reference the same literal instead of
/// duplicating magic strings.
/// </summary>
public static class Roles
{
    public const string JobSeeker = "JobSeeker";
    public const string Company = "Company";
    public const string Admin = "Admin";
}
