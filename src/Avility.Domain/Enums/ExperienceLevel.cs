namespace Avility.Domain.Enums;

/// <summary>
/// Not part of the original property list - added to give JobPosting's
/// ExperienceLevel field a concrete type instead of a raw string, keeping
/// it consistent with the other status/category fields on the model.
/// </summary>
public enum ExperienceLevel
{
    EntryLevel,
    Junior,
    MidLevel,
    Senior,
    Lead,
    Executive
}
