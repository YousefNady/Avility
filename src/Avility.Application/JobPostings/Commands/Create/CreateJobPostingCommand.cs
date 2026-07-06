using Avility.Application.JobPostings.Dtos;
using MediatR;

namespace Avility.Application.JobPostings.Commands.Create;

public sealed record CreateJobPostingCommand(
    string Title,
    string Description,
    string? Requirements,
    string EmploymentType,
    string ExperienceLevel,
    bool IsRemote,
    string? Country,
    string? Governorate,
    string? City,
    decimal? SalaryMin,
    decimal? SalaryMax,
    string? SalaryCurrency,
    DateTime? ApplicationDeadline) : IRequest<JobPostingDto>;
