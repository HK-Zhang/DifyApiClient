using DifyApiClient.Models;
using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;

namespace DifyApiClient.Tests;

public class DifyApiClientTests : IDisposable
{
    private readonly MockHttpMessageHandler _mockHandler;
    private readonly HttpClient _httpClient;
    private readonly DifyApiClient _client;

    public DifyApiClientTests()
    {
        _mockHandler = new MockHttpMessageHandler();
        _httpClient = new HttpClient(_mockHandler);
        
        var options = new DifyApiClientOptions
        {
            BaseUrl = "http://localhost/v1",
            ApiKey = "test-api-key"
        };

        _client = new DifyApiClient(options, _httpClient);
    }

    [Fact]
    public async Task SendChatMessageAsync_ReturnsValidResponse()
    {
        // Arrange
        var expectedResponse = new ChatCompletionResponse
        {
            MessageId = "msg-123",
            ConversationId = "conv-456",
            Answer = "Hello! How can I help you?",
            CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };

        _mockHandler.SetupResponse(HttpStatusCode.OK, expectedResponse);

        var request = new ChatMessageRequest
        {
            Query = "Hello",
            User = "user-123"
        };

        // Act
        var result = await _client.SendChatMessageAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedResponse.MessageId, result.MessageId);
        Assert.Equal(expectedResponse.Answer, result.Answer);
    }

    [Fact]
    public async Task GetConversationsAsync_ReturnsPaginatedList()
    {
        // Arrange
        var expectedResponse = new ConversationListResponse
        {
            Data = new List<Conversation>
            {
                new() { Id = "conv-1", Name = "Conversation 1", CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
                new() { Id = "conv-2", Name = "Conversation 2", CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds() }
            },
            HasMore = false,
            Limit = 20,
            Page = 1
        };

        _mockHandler.SetupResponse(HttpStatusCode.OK, expectedResponse);

        // Act
        var result = await _client.GetConversationsAsync("user-123");

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Count);
        Assert.Equal("conv-1", result.Data[0].Id);
    }

    [Fact]
    public async Task UploadFileAsync_ReturnsFileInfo()
    {
        // Arrange
        var expectedResponse = new FileUploadResponse
        {
            Id = "file-123",
            Name = "test.txt",
            Size = 1024,
            Extension = "txt",
            MimeType = "text/plain",
            CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };

        _mockHandler.SetupResponse(HttpStatusCode.OK, expectedResponse);

        using var fileStream = new MemoryStream(Encoding.UTF8.GetBytes("Test content"));

        // Act
        var result = await _client.UploadFileAsync(fileStream, "test.txt", "user-123");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedResponse.Id, result.Id);
        Assert.Equal(expectedResponse.Name, result.Name);
    }

    [Fact]
    public async Task GetApplicationInfoAsync_ReturnsApplicationInfo()
    {
        // Arrange
        var expectedResponse = new ApplicationInfo
        {
            Name = "Test App",
            Description = "Test Description",
            Mode = "chat",
            Tags = new List<string> { "test", "demo" }
        };

        _mockHandler.SetupResponse(HttpStatusCode.OK, expectedResponse);

        // Act
        var result = await _client.GetApplicationInfoAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedResponse.Name, result.Name);
        Assert.Equal(expectedResponse.Description, result.Description);
        Assert.Equal(2, result.Tags?.Count);
    }

    [Fact]
    public async Task CreateAnnotationAsync_ReturnsCreatedAnnotation()
    {
        // Arrange
        var expectedResponse = new Annotation
        {
            Id = "annotation-123",
            Question = "What is your name?",
            Answer = "I am Dify",
            HitCount = 0,
            CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };

        _mockHandler.SetupResponse(HttpStatusCode.OK, expectedResponse);

        var request = new AnnotationRequest
        {
            Question = "What is your name?",
            Answer = "I am Dify"
        };

        // Act
        var result = await _client.CreateAnnotationAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedResponse.Id, result.Id);
        Assert.Equal(expectedResponse.Question, result.Question);
        Assert.Equal(expectedResponse.Answer, result.Answer);
    }

    [Fact]
    public async Task DeleteConversationAsync_CompletesSuccessfully()
    {
        // Arrange
        _mockHandler.SetupResponse(HttpStatusCode.NoContent, (object?)null);

        // Act & Assert
        await _client.DeleteConversationAsync("conv-123", "user-123");
    }

    [Fact]
    public async Task SetAnnotationReplyAsync_ValidatesAction()
    {
        // Arrange & Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _client.SetAnnotationReplyAsync("invalid-action"));
    }

    [Fact]
    public async Task SendChatMessageAsync_IncludesAuthorizationHeader()
    {
        // Arrange
        _mockHandler.SetupResponse(HttpStatusCode.OK, new ChatCompletionResponse { Answer = "Test" });

        var request = new ChatMessageRequest
        {
            Query = "Test",
            User = "user-123"
        };

        // Act
        await _client.SendChatMessageAsync(request);

        // Assert
        var lastRequest = _mockHandler.LastRequest;
        Assert.NotNull(lastRequest);
        Assert.True(lastRequest.Headers.Contains("Authorization"));
        Assert.Equal("Bearer test-api-key", lastRequest.Headers.GetValues("Authorization").First());
    }

    public void Dispose()
    {
        _client?.Dispose();
        _httpClient?.Dispose();
        _mockHandler?.Dispose();
        GC.SuppressFinalize(this);
    }
}
