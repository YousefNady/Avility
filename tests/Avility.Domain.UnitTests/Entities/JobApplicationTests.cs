using Avility.Domain.Entities;
using Avility.Domain.Enums;
using Avility.Domain.Exceptions;
using Xunit;

namespace Avility.Domain.UnitTests.Entities;

public class JobApplicationTests
{
    [Fact]
    public void Create_WithValidData_Succeeds()
    {
        var application = JobApplication.Create(Guid.NewGuid(), Guid.NewGuid(), "Cover letter", DateTime.UtcNow);

        Assert.Equal(ApplicationStatus.Applied, application.Status);
    }

    [Fact]
    public void Create_WithEmptyJobSeekerId_Throws()
    {
        Assert.Throws<DomainValidationException>(() =>
            JobApplication.Create(Guid.Empty, Guid.NewGuid(), null, DateTime.UtcNow));
    }

    [Fact]
    public void MoveToUnderReview_FromApplied_Succeeds()
    {
        var application = JobApplication.Create(Guid.NewGuid(), Guid.NewGuid(), null, DateTime.UtcNow);

        application.MoveToUnderReview();

        Assert.Equal(ApplicationStatus.UnderReview, application.Status);
    }

    [Fact]
    public void Accept_FromApplied_Succeeds()
    {
        var application = JobApplication.Create(Guid.NewGuid(), Guid.NewGuid(), null, DateTime.UtcNow);

        application.Accept();

        Assert.Equal(ApplicationStatus.Accepted, application.Status);
    }

    [Fact]
    public void Withdraw_FromApplied_Succeeds()
    {
        var application = JobApplication.Create(Guid.NewGuid(), Guid.NewGuid(), null, DateTime.UtcNow);

        application.Withdraw();

        Assert.Equal(ApplicationStatus.Withdrawn, application.Status);
    }

    [Fact]
    public void Accept_AfterWithdrawn_Throws()
    {
        var application = JobApplication.Create(Guid.NewGuid(), Guid.NewGuid(), null, DateTime.UtcNow);
        application.Withdraw();

        Assert.Throws<InvalidStatusTransitionException>(() => application.Accept());
    }

    [Fact]
    public void Withdraw_AfterAccepted_Throws()
    {
        var application = JobApplication.Create(Guid.NewGuid(), Guid.NewGuid(), null, DateTime.UtcNow);
        application.Accept();

        Assert.Throws<InvalidStatusTransitionException>(() => application.Withdraw());
    }

    [Fact]
    public void Reject_FromUnderReview_Succeeds()
    {
        var application = JobApplication.Create(Guid.NewGuid(), Guid.NewGuid(), null, DateTime.UtcNow);
        application.MoveToUnderReview();

        application.Reject();

        Assert.Equal(ApplicationStatus.Rejected, application.Status);
    }
}
