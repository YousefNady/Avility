namespace Avility.Domain.Enums;

/// <summary>
/// Categorized by topic, not by content format (article/video/PDF/course).
/// The same URL can be any format, so format isn't a useful filter axis -
/// what matters to a job seeker is what the resource is about.
/// </summary>
public enum ResourceCategory
{
    CareerAdvice,
    InterviewPreparation,
    ResumeWriting,
    WorkplaceAccommodations,
    SkillDevelopment,
    MentalHealthAndWellbeing,
    Other
}