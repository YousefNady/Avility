using Avility.Domain.Entities;
using Avility.Domain.Enums;
using Avility.Domain.Exceptions;
using Avility.Domain.ValueObjects;
using Xunit;

namespace Avility.Domain.UnitTests.Entities;

public class JobPostingTests
{
    private static JobPosting ValidPosting(DateTime? deadline = null) => JobPosting.Create(
        Guid.NewGuid(), "Backend Engineer", "Build APIs", EmploymentType.FullTime, ExperienceLevel.MidLevel,
        false, Location.Create("Egypt", "Giza", "Giza"), null, deadline);

    [Fact]
    public void Create_Remote_WithoutLocation_Succeeds()
    {
        var posting = JobPosting.Create(
            Guid.NewGuid(), "Remote Dev", "Build things", EmploymentType.FullTime, ExperienceLevel.Senior,
            true, null, null, null);

        Assert.Null(posting.Location);
        Assert.Equal(JobPostingStatus.Draft, posting.Status);
    }

    [Fact]
    public void Create_OnSite_WithoutLocation_Throws()
    {
        Assert.Throws<DomainValidationException>(() =>
            JobPosting.Create(Guid.NewGuid(), "Dev", "Desc", EmploymentType.FullTime, ExperienceLevel.MidLevel, false, null, null, null));
    }

    [Fact]
    public void Publish_FromDraft_Succeeds()
    {
        var posting = ValidPosting();

        posting.Publish(DateTime.UtcNow);

        Assert.Equal(JobPostingStatus.Published, posting.Status);
        Assert.NotNull(posting.PublishedAt);
    }

    [Fact]
    public void Publish_Twice_Throws()
    {
        var posting = ValidPosting();
        posting.Publish(DateTime.UtcNow);

        Assert.Throws<InvalidStatusTransitionException>(() => posting.Publish(DateTime.UtcNow));
    }

    [Fact]
    public void Publish_WithPastDeadline_Throws()
    {
        var posting = ValidPosting(DateTime.UtcNow.AddDays(-1));

        Assert.Throws<DomainValidationException>(() => posting.Publish(DateTime.UtcNow));
    }

    [Fact]
    public void Close_DraftDirectly_Succeeds()
    {
        var posting = ValidPosting();

        posting.Close(DateTime.UtcNow);

        Assert.Equal(JobPostingStatus.Closed, posting.Status);
    }

    [Fact]
    public void Close_AlreadyClosed_Throws()
    {
        var posting = ValidPosting();
        posting.Close(DateTime.UtcNow);

        Assert.Throws<InvalidStatusTransitionException>(() => posting.Close(DateTime.UtcNow));
    }

    [Fact]
    public void UpdateDetails_WhenClosed_Throws()
    {
        var posting = ValidPosting();
        posting.Close(DateTime.UtcNow);

        Assert.Throws<DomainValidationException>(() =>
            posting.UpdateDetails("New Title", "New Desc", null, EmploymentType.FullTime, ExperienceLevel.MidLevel,
                false, Location.Create("Egypt", "Giza", "Giza"), null, null));
    }

    [Fact]
    public void CanAcceptApplications_OnlyWhenPublishedAndBeforeDeadline()
    {
        var posting = ValidPosting(DateTime.UtcNow.AddDays(5));
        posting.Publish(DateTime.UtcNow);

        Assert.True(posting.CanAcceptApplications(DateTime.UtcNow));
        Assert.False(posting.CanAcceptApplications(DateTime.UtcNow.AddDays(10)));
    }
    
    [Fact]
        public void UpdateAccommodations_OnClosedPosting_Throws()
        {
            var posting = ValidPosting();
            posting.Close(DateTime.UtcNow);
    
            Assert.Throws<DomainValidationException>(() =>
                posting.UpdateAccommodations(new[] { DisabilityCategory.Mobility }, "Wheelchair accessible office"));
        }
}
