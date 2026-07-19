using Avility.Application.Companies.Dtos;
using Avility.Application.JobSeekers.Dtos;

namespace Avility.Application.Admin.Queries.GetUserDetails;

public sealed record UserDetailsDto(
    Guid Id,
    string Email,
    IReadOnlyList<string> Roles,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? LastLoginAt,
    JobSeekerProfileDto? JobSeekerProfile,
    CompanyProfileDto? CompanyProfile);