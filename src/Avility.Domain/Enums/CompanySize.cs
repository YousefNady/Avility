namespace Avility.Domain.Enums;

/// <summary>
/// Modeled as a fixed set of bands rather than a raw employee-count
/// integer, matching how company size is conventionally presented on
/// recruitment platforms (and avoiding the ambiguity of a free-form number
/// that's constantly out of date).
/// </summary>
public enum CompanySize
{
    OneToTen,
    ElevenToFifty,
    FiftyOneToTwoHundred,
    TwoHundredOneToFiveHundred,
    FiveHundredOneToThousand,
    MoreThanThousand
}
