using Avility.Application.JobSeekers.Dtos;
using MediatR;

namespace Avility.Application.JobSeekers.Commands.CreateProfile;

public sealed record CreateJobSeekerProfileCommand(
    string FullName,
    string PhoneNumber,
    int YearsOfExperience,
    string CurrentJobTitle,
    string Country,
    string Governorate,
    string City) : IRequest<JobSeekerProfileDto>;
