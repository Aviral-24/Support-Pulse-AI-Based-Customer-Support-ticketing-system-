using System.Net;
using Microsoft.Extensions.Configuration;
using Pgvector;
using Backend.Services;
using Xunit;

namespace Units_Tests;

public class MockFailedHttpMessageHandler : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Simulate a severe network crash or HuggingFace API offline scenario
        throw new HttpRequestException("HuggingFace API is currently unreachable!");
    }
}

public class AiServiceTests
{
    [Fact]
    public async Task GenerateEmbeddingAsync_WhenApiFails_ReturnsGracefulFallbackVector()
    {
        // 1. ARRANGE: Configuration aur fake HTTP Client setup karna
        var inMemorySettings = new Dictionary<string, string?> 
        {
            {"Groq:ApiKey", "test-groq-key"},
            {"HuggingFace:ApiKey", "test-hf-key"}
        };
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        // Apna nakli (crash hone wala) HTTP Client service me inject karna
        var mockHandler = new MockFailedHttpMessageHandler();
        var fakeHttpClient = new HttpClient(mockHandler);

        var aiService = new AiService(configuration, fakeHttpClient);

        // ACT: Method call karna
        // Jaise hi ye call hoga, fakeHttpClient error fekega, aur AiService ka 'catch' block activate hona chahiye
        var result = await aiService.GenerateEmbeddingAsync("Test ticket text for AI");

        //  ASSERT: Verify karna ki system crash NAE hua, aur safe fallback vector wapas aaya
        Assert.NotNull(result);

        var vectorArray = result.ToArray();
        
        // Fallback vector ka size exactly 384 hona chahiye (Database error rokne ke liye)
        Assert.Equal(384, vectorArray.Length);
        
        // Check 2: DivideByZero error rokne ke liye Index 0 ki value 1.0f honi chahiye
        Assert.Equal(1.0f, vectorArray[0]);
        
        //  Baki ki values 0 honi chahiye
        Assert.Equal(0.0f, vectorArray[1]);
        Assert.Equal(0.0f, vectorArray[383]);
    }
}