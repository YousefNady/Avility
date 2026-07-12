using Avility.Domain.Entities;
using Avility.Domain.Exceptions;
using Xunit;

namespace Avility.Domain.UnitTests.Entities;

public class MessageTests
{
    [Fact]
    public void Create_WithValidData_Succeeds()
    {
        var message = Message.Create(Guid.NewGuid(), Guid.NewGuid(), "Hello, when can you start?");
        Assert.Equal("Hello, when can you start?", message.Body);
    }

    [Fact]
    public void Create_WithEmptyBody_Throws()
    {
        Assert.Throws<DomainValidationException>(() => Message.Create(Guid.NewGuid(), Guid.NewGuid(), "   "));
    }

    [Fact]
    public void Create_WithEmptyJobApplicationId_Throws()
    {
        Assert.Throws<DomainValidationException>(() => Message.Create(Guid.Empty, Guid.NewGuid(), "Hello"));
    }
}