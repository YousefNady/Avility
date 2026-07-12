using Avility.Domain.Entities;
using Avility.Domain.Exceptions;
using Xunit;

namespace Avility.Domain.UnitTests.Entities;

public class RefreshTokenTests
{
    [Fact]
    public void Create_WithValidData_Succeeds()
    {
        var token = RefreshToken.Create(Guid.NewGuid(), "hash", DateTime.UtcNow.AddDays(7), DateTime.UtcNow);

        Assert.False(token.IsRevoked);
    }

    [Fact]
    public void Create_WithExpiryBeforeCreation_Throws()
    {
        var now = DateTime.UtcNow;
        Assert.Throws<DomainValidationException>(() => RefreshToken.Create(Guid.NewGuid(), "hash", now.AddDays(-1), now));
    }

    [Fact]
    public void IsActive_WhenNotRevokedAndNotExpired_ReturnsTrue()
    {
        var now = DateTime.UtcNow;
        var token = RefreshToken.Create(Guid.NewGuid(), "hash", now.AddDays(7), now);

        Assert.True(token.IsActive(now));
    }

    [Fact]
    public void IsActive_WhenExpired_ReturnsFalse()
    {
        var now = DateTime.UtcNow;
        var token = RefreshToken.Create(Guid.NewGuid(), "hash", now.AddMinutes(1), now);

        Assert.False(token.IsActive(now.AddMinutes(5)));
    }

    [Fact]
    public void Revoke_Twice_Throws()
    {
        var now = DateTime.UtcNow;
        var token = RefreshToken.Create(Guid.NewGuid(), "hash", now.AddDays(7), now);
        token.Revoke(now);

        Assert.Throws<DomainValidationException>(() => token.Revoke(now));
    }
}
