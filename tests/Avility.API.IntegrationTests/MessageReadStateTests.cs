using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Avility.API.Common.Responses;
using Avility.Application.Auth;
using Avility.Application.Messages.Dtos;
using Xunit;

namespace Avility.API.IntegrationTests;

public class MessageReadStateTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public MessageReadStateTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    // NOTE: reuse your real MessagingTests.cs helpers
    // (SetUpVerifiedCompanyWithPublishedPostingAsync / AuthTokenAsync /
    // ApplyAsJobSeekerAsync) here rather than the placeholders below -
    // matching your actual file's names precisely, same caveat as prior
    // milestones.

    [Fact]
    public async Task MarkAsRead_ThenGetThread_ReflectsReadState()
    {
        // Arrange: company + published posting + seeker application
        // (mirror MessagingTests.cs setup), seeker sends a message,
        // company reads it.

        // Act: POST {id}/messages/read as Company
        // Assert: GET {id}/messages shows isRead=true, readAt != null
        // for the seeker's message
    }

    [Fact]
    public async Task GetUnreadCounts_ReflectsUnreadMessage_ThenZeroAfterRead()
    {
        // Arrange as above.
        // Act 1: GET /jobapplications/unread-counts as Company -> should
        // include this jobApplicationId with UnreadCount >= 1.
        // Act 2: POST {id}/messages/read as Company.
        // Act 3: GET /jobapplications/unread-counts again as Company ->
        // should no longer include this jobApplicationId.
    }
}