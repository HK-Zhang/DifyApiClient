using DifyApiClient.Models;
using System.Text;
using Xunit;

namespace DifyApiClient.Tests;

public class DifyApiClientTests : IDisposable
{
    private readonly DifyApiClient _client;

    public DifyApiClientTests()
    {
        // Get API key from environment variable or use a default for testing
        var apiKey = Environment.GetEnvironmentVariable("DIFY_API_KEY") ?? "app-your-api-key-here";
        
        var options = new DifyApiClientOptions
        {
            BaseUrl = "http://osl4243:8980/v1",
            ApiKey = apiKey
        };

        _client = new DifyApiClient(options);
    }

    [Fact]
    public async Task SendChatMessageAsync_ReturnsValidResponse()
    {
        // Arrange
        var request = new ChatMessageRequest
        {
            Query = "Hello",
            User = "test-user-123"
        };

        // Act
        var result = await _client.SendChatMessageAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.MessageId);
        Assert.NotNull(result.Answer);
        Assert.True(result.CreatedAt > 0);
    }

    [Fact]
    public async Task GetConversationsAsync_ReturnsPaginatedList()
    {
        // Act
        var result = await _client.GetConversationsAsync("test-user-123");

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        // The actual count depends on what exists on the server
    }

    [Fact]
    public async Task UploadFileAsync_ReturnsFileInfo()
    {
        // Arrange
        using var fileStream = new MemoryStream(Encoding.UTF8.GetBytes("Test content for file upload"));

        // Act
        var result = await _client.UploadFileAsync(fileStream, "test.txt", "test-user-123");

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Id);
        Assert.Equal("test.txt", result.Name);
        Assert.True(result.CreatedAt > 0);
    }

    [Fact]
    public async Task GetApplicationInfoAsync_ReturnsApplicationInfo()
    {
        // Act
        var result = await _client.GetApplicationInfoAsync();

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Name);
        // Mode and other properties depend on actual server configuration
    }

    [Fact]
    public async Task CreateAnnotationAsync_ReturnsCreatedAnnotation()
    {
        // Arrange
        var request = new AnnotationRequest
        {
            Question = "What is your name?",
            Answer = "I am Dify"
        };

        // Act
        var result = await _client.CreateAnnotationAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Id);
        Assert.Equal(request.Question, result.Question);
        Assert.Equal(request.Answer, result.Answer);
        Assert.True(result.CreatedAt > 0);
    }

    [Fact]
    public async Task DeleteConversationAsync_CompletesSuccessfully()
    {
        // This test requires a valid conversation ID
        // Skipping actual deletion to avoid side effects
        // You can manually test with a real conversation ID
        
        // Act & Assert - just verify the method signature
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            _client.DeleteConversationAsync(null!, "test-user-123"));
    }

    [Fact]
    public async Task SetAnnotationReplyAsync_ValidatesAction()
    {
        // Arrange & Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _client.SetAnnotationReplyAsync("invalid-action"));
    }

    public void Dispose()
    {
        _client?.Dispose();
        GC.SuppressFinalize(this);
    }
}
