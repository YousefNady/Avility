using System.Collections.Generic;
using Avility.Infrastructure.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Avility.API.IntegrationTests;

/// <summary>
/// Doesn't boot the full API host (that would just prove the factory's
/// own override works, which every other test already does implicitly).
/// Instead builds a minimal, isolated options pipeline with the exact
/// same Validate() rule, using the real placeholder value, to prove the
/// guard itself actually rejects what it's meant to reject.
/// </summary>
public class JwtSecretValidationTests
{
    [Fact]
    public void PlaceholderSecret_FailsValidation()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"] = "REPLACE_WITH_USER_SECRET_MIN_32_CHARS"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddOptions<JwtSettings>()
            .Bind(configuration.GetSection(JwtSettings.SectionName))
            .Validate(
                settings => !string.IsNullOrWhiteSpace(settings.Secret)
                    && settings.Secret.Length >= 32
                    && settings.Secret != "REPLACE_WITH_USER_SECRET_MIN_32_CHARS",
                "placeholder rejected");

        var provider = services.BuildServiceProvider();

        Assert.Throws<OptionsValidationException>(() => provider.GetRequiredService<IOptions<JwtSettings>>().Value);
    }
}