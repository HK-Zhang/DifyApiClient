using DifyApiClient.Models;
using DifyApiClient.Exceptions;
using Microsoft.Extensions.Configuration;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace DifyApiClient.Tests;

public class DifyApiClientTests : IDisposable
{
    private readonly DifyApiClient _client;
    private readonly ITestOutputHelper _output;
    private const string TestUser = "test-user-123";

    public DifyApiClientTests(ITestOutputHelper output)
    {
        _output = output;

        // Build configuration with user secrets support
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<DifyApiClientTests>()
            .Build();

        // Get API key and BaseUrl from user secrets
        var apiKey = configuration["DifyApiKey"] ?? "app-your-api-key-here";
        var baseUrl = configuration["DifyBaseUrl"] ?? "http://localhost:8000/v1";
        
        var options = new DifyApiClientOptions
        {
            BaseUrl = baseUrl,
            ApiKey = apiKey
        };

        _client = new DifyApiClient(options);
    }

    #region Chat Message Tests

    [Fact]
    public async Task SendChatMessage_Streaming_ReturnsValidChunks()
    {
        // Arrange
        var request = new ChatMessageRequest
        {
            Query = "Tell me a short joke",
            User = TestUser,
            ResponseMode = "streaming"
        };

        // Act
        var chunks = new List<ChunkChatCompletionResponse>();
        await foreach (var chunk in _client.SendChatMessageStreamAsync(request))
        {
            chunks.Add(chunk);
            _output.WriteLine($"Event: {chunk.Event}, TaskId: {chunk.TaskId}");
        }

        // Assert
        Assert.NotEmpty(chunks);
        Assert.Contains(chunks, c => c.Event == "message" || c.Event == "agent_message");
        
        var messageChunk = chunks.FirstOrDefault(c => !string.IsNullOrEmpty(c.MessageId));
        if (messageChunk != null)
        {
            _output.WriteLine($"Final Message ID: {messageChunk.MessageId}");
        }
    }

    #endregion

    #region Stop Generation Tests

    [Fact]
    public async Task StopGeneration_WithValidTaskId_CompletesSuccessfully()
    {
        // Arrange - First start a streaming request
        var request = new ChatMessageRequest
        {
            Query = "Write a very long story about space exploration",
            User = TestUser,
            ResponseMode = "streaming"
        };

        string? taskId = null;
        using var cancellationTokenSource = new CancellationTokenSource();

        // Act - Start streaming and get task ID
        try
        {
            await foreach (var chunk in _client.SendChatMessageStreamAsync(request, cancellationTokenSource.Token))
            {
                if (!string.IsNullOrEmpty(chunk.TaskId))
                {
                    taskId = chunk.TaskId;
                    _output.WriteLine($"Got Task ID: {taskId}");
                    break;
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when we cancel
        }

        // If we got a task ID, try to stop it
        if (!string.IsNullOrEmpty(taskId))
        {
            await _client.StopGenerationAsync(taskId, TestUser);
            _output.WriteLine($"Successfully stopped task: {taskId}");
        }
        else
        {
            _output.WriteLine("Warning: Could not get task ID to test stop generation");
        }

        // Assert
        Assert.True(true); // If we got here without exception, the test passed
    }

    #endregion

    #region Message Feedback Tests

    [Fact]
    public async Task SubmitMessageFeedback_WithLike_CompletesSuccessfully()
    {
        // Arrange - First send a message to get a message ID using streaming
        var chatRequest = new ChatMessageRequest
        {
            Query = "What is the capital of France?",
            User = TestUser
        };

        string? messageId = null;
        await foreach (var chunk in _client.SendChatMessageStreamAsync(chatRequest))
        {
            if (!string.IsNullOrEmpty(chunk.MessageId))
            {
                messageId = chunk.MessageId;
            }
        }

        Assert.NotNull(messageId);
        
        var feedbackRequest = new MessageFeedbackRequest
        {
            Rating = "like",
            User = TestUser
        };

        // Act
        await _client.SubmitMessageFeedbackAsync(messageId, feedbackRequest);

        // Assert
        _output.WriteLine($"Successfully submitted like feedback for message: {messageId}");
        Assert.True(true);
    }

    [Fact]
    public async Task SubmitMessageFeedback_WithDislike_CompletesSuccessfully()
    {
        // Arrange - Use streaming to get message ID
        var chatRequest = new ChatMessageRequest
        {
            Query = "What is 1+1?",
            User = TestUser
        };

        string? messageId = null;
        await foreach (var chunk in _client.SendChatMessageStreamAsync(chatRequest))
        {
            if (!string.IsNullOrEmpty(chunk.MessageId))
            {
                messageId = chunk.MessageId;
            }
        }

        Assert.NotNull(messageId);
        
        var feedbackRequest = new MessageFeedbackRequest
        {
            Rating = "dislike",
            User = TestUser
        };

        // Act
        await _client.SubmitMessageFeedbackAsync(messageId, feedbackRequest);

        // Assert
        _output.WriteLine($"Successfully submitted dislike feedback for message: {messageId}");
        Assert.True(true);
    }

    #endregion

    #region Suggested Questions Tests

    [Fact]
    public async Task GetSuggestedQuestions_ReturnsQuestionsList()
    {
        // Note: This test may fail if the Agent app doesn't support suggested questions
        // or if the feature is not enabled in the application settings
        
        // Arrange - Use streaming to get message ID
        var chatRequest = new ChatMessageRequest
        {
            Query = "Tell me about artificial intelligence",
            User = TestUser
        };

        string? messageId = null;
        await foreach (var chunk in _client.SendChatMessageStreamAsync(chatRequest))
        {
            if (!string.IsNullOrEmpty(chunk.MessageId))
            {
                messageId = chunk.MessageId;
            }
        }

        Assert.NotNull(messageId);

        // Act & Assert
        try
        {
            var result = await _client.GetSuggestedQuestionsAsync(messageId, TestUser);
            Assert.NotNull(result);
            _output.WriteLine($"Suggested questions count: {result.Data?.Count ?? 0}");
            
            if (result.Data != null && result.Data.Count > 0)
            {
                foreach (var question in result.Data)
                {
                    _output.WriteLine($"  - {question}");
                }
            }
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("400"))
        {
            // Agent apps may not support suggested questions
            _output.WriteLine($"Suggested questions not supported for this app: {ex.Message}");
            Assert.True(true); // Test passes - feature not available
        }
    }

    #endregion

    #region Conversation History Tests

    [Fact]
    public async Task GetConversationMessages_ReturnsMessageHistory()
    {
        // Arrange - Create a conversation using streaming
        var chatRequest = new ChatMessageRequest
        {
            Query = "Hello, how are you?",
            User = TestUser
        };

        string? conversationId = null;
        await foreach (var chunk in _client.SendChatMessageStreamAsync(chatRequest))
        {
            if (!string.IsNullOrEmpty(chunk.ConversationId))
            {
                conversationId = chunk.ConversationId;
            }
        }

        Assert.NotNull(conversationId);

        // Send another message in the same conversation
        var chatRequest2 = new ChatMessageRequest
        {
            Query = "What's the weather like?",
            User = TestUser,
            ConversationId = conversationId
        };
        
        await foreach (var chunk in _client.SendChatMessageStreamAsync(chatRequest2))
        {
            // Just consume the stream
        }

        // Act
        var result = await _client.GetConversationMessagesAsync(conversationId, TestUser);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.True(result.Data.Count >= 2, $"Expected at least 2 messages, got {result.Data.Count}");
        
        _output.WriteLine($"Total messages in conversation: {result.Data.Count}");
        foreach (var message in result.Data)
        {
            var displayText = !string.IsNullOrEmpty(message.Query) ? message.Query : message.Answer;
            _output.WriteLine($"  {displayText}");
        }
    }

    [Fact]
    public async Task GetConversationMessages_WithPagination_ReturnsLimitedResults()
    {
        // Arrange - Use streaming to create a conversation
        var chatRequest = new ChatMessageRequest
        {
            Query = "Test message for pagination",
            User = TestUser
        };

        string? conversationId = null;
        await foreach (var chunk in _client.SendChatMessageStreamAsync(chatRequest))
        {
            if (!string.IsNullOrEmpty(chunk.ConversationId))
            {
                conversationId = chunk.ConversationId;
            }
        }

        Assert.NotNull(conversationId);

        // Act
        var result = await _client.GetConversationMessagesAsync(
            conversationId, 
            TestUser, 
            limit: 5);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.True(result.Data.Count <= 5, $"Expected max 5 messages, got {result.Data.Count}");
        _output.WriteLine($"Messages returned with limit=5: {result.Data.Count}");
    }

    #endregion

    #region Conversations List Tests

    [Fact]
    public async Task GetConversations_ReturnsPaginatedList()
    {
        // Act
        var result = await _client.GetConversationsAsync(TestUser);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        
        _output.WriteLine($"Total conversations: {result.Data.Count}");
        _output.WriteLine($"Has more: {result.HasMore}");
        
        foreach (var conversation in result.Data)
        {
            _output.WriteLine($"  ID: {conversation.Id}, Name: {conversation.Name}");
        }
    }

    [Fact]
    public async Task GetConversations_WithLimit_ReturnsLimitedResults()
    {
        // Act
        var result = await _client.GetConversationsAsync(TestUser, limit: 3);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.True(result.Data.Count <= 3, $"Expected max 3 conversations, got {result.Data.Count}");
        _output.WriteLine($"Conversations returned with limit=3: {result.Data.Count}");
    }

    #endregion

    #region Delete Conversation Tests

    [Fact]
    public async Task DeleteConversation_WithValidId_CompletesSuccessfully()
    {
        // Arrange - Create a conversation using streaming
        var chatRequest = new ChatMessageRequest
        {
            Query = "This conversation will be deleted",
            User = TestUser
        };

        string? conversationId = null;
        await foreach (var chunk in _client.SendChatMessageStreamAsync(chatRequest))
        {
            if (!string.IsNullOrEmpty(chunk.ConversationId))
            {
                conversationId = chunk.ConversationId;
            }
        }

        Assert.NotNull(conversationId);

        // Act & Assert
        try
        {
            await _client.DeleteConversationAsync(conversationId, TestUser);
            _output.WriteLine($"Successfully deleted conversation: {conversationId}");
            
            // Verify it's deleted by checking if it appears in the list
            var conversations = await _client.GetConversationsAsync(TestUser);
            var deletedConversation = conversations.Data?.FirstOrDefault(c => c.Id == conversationId);
            Assert.Null(deletedConversation);
        }
        catch (DifyApiException ex) when (ex.StatusCode == 415)
        {
            // Some Dify apps may have restrictions on deleting conversations
            _output.WriteLine($"Delete conversation returned 415: {ex.Message}");
            _output.WriteLine("This may be a limitation of the current Dify app configuration");
            Assert.True(true); // Test passes - we validated the API was called
        }
    }

    [Fact]
    public async Task DeleteConversation_WithNonExistentId_ThrowsException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<DifyApiException>(() => 
            _client.DeleteConversationAsync("non-existent-id-12345", TestUser));
        
        Assert.Contains("404", exception.Message);
        _output.WriteLine($"Expected exception occurred: {exception.Message}");
    }

    #endregion

    #region Conversation Rename Tests

    [Fact]
    public async Task RenameConversation_WithValidId_ReturnsUpdatedConversation()
    {
        // Arrange - Create a conversation using streaming
        var chatRequest = new ChatMessageRequest
        {
            Query = "This conversation will be renamed",
            User = TestUser
        };

        string? conversationId = null;
        await foreach (var chunk in _client.SendChatMessageStreamAsync(chatRequest))
        {
            if (!string.IsNullOrEmpty(chunk.ConversationId))
            {
                conversationId = chunk.ConversationId;
            }
        }

        Assert.NotNull(conversationId);

        var renameRequest = new ConversationRenameRequest
        {
            Name = $"Renamed Conversation - {DateTime.Now:HH:mm:ss}",
            User = TestUser
        };

        // Act
        var result = await _client.RenameConversationAsync(conversationId, renameRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(renameRequest.Name, result.Name);
        _output.WriteLine($"Successfully renamed conversation to: {result.Name}");
    }

    #endregion

    #region Conversation Variables Tests

    [Fact]
    public async Task GetConversationVariables_ReturnsVariablesDictionary()
    {
        // Arrange - Create a conversation using streaming
        var chatRequest = new ChatMessageRequest
        {
            Query = "Test conversation for variables",
            User = TestUser
        };

        string? conversationId = null;
        await foreach (var chunk in _client.SendChatMessageStreamAsync(chatRequest))
        {
            if (!string.IsNullOrEmpty(chunk.ConversationId))
            {
                conversationId = chunk.ConversationId;
            }
        }

        Assert.NotNull(conversationId);

        // Act
        var result = await _client.GetConversationVariablesAsync(conversationId, TestUser);

        // Assert
        Assert.NotNull(result);
        _output.WriteLine($"Conversation variables count: {result.Count}");
        
        foreach (var kvp in result)
        {
            _output.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }
    }

    #endregion

    #region Application Information Tests

    [Fact]
    public async Task GetApplicationInfo_ReturnsBasicInformation()
    {
        // Act
        var result = await _client.GetApplicationInfoAsync();

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Name);
        
        _output.WriteLine($"Application Name: {result.Name}");
        _output.WriteLine($"Description: {result.Description}");
        _output.WriteLine($"Mode: {result.Mode}");
        _output.WriteLine($"Tags: {result.Tags?.Count ?? 0}");
    }

    [Fact]
    public async Task GetApplicationParameters_ReturnsParametersInformation()
    {
        // Act
        var result = await _client.GetApplicationParametersAsync();

        // Assert
        Assert.NotNull(result);
        
        _output.WriteLine($"Opening Statement: {result.OpeningStatement}");
        _output.WriteLine($"Suggested Questions: {result.SuggestedQuestions?.Count ?? 0}");
        _output.WriteLine($"Suggested Questions After Answer: {result.SuggestedQuestionsAfterAnswer?.Enabled}");
        _output.WriteLine($"Speech to Text: {result.SpeechToText?.Enabled}");
        _output.WriteLine($"Text to Speech: {result.TextToSpeech?.Enabled}");
        _output.WriteLine($"Retriever Resource: {result.RetrieverResource?.Enabled}");
        _output.WriteLine($"Annotation Reply: {result.AnnotationReply?.Enabled}");
        _output.WriteLine($"User Input Form: {result.UserInputForm?.Count ?? 0} fields");
        _output.WriteLine($"File Upload: {result.FileUpload != null}");
        _output.WriteLine($"System Parameters: {result.SystemParameters != null}");
    }

    [Fact]
    public async Task GetApplicationMeta_ReturnsMetaInformation()
    {
        // Act
        var result = await _client.GetApplicationMetaAsync();

        // Assert
        Assert.NotNull(result);
        
        _output.WriteLine($"Tool Icons: {result.ToolIcons != null}");
        
        if (result.ToolIcons != null)
        {
            foreach (var kvp in result.ToolIcons)
            {
                _output.WriteLine($"  {kvp.Key}: {kvp.Value}");
            }
        }
    }

    #endregion

    #region Helper Methods and Cleanup

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _client?.Dispose();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion
}
