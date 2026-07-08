namespace Avility.Application.JobSeekers.Dtos;

public sealed record ResumeFileResult(Stream Content, string ContentType, string FileName);