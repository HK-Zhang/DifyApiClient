# Migration Guide - Refactored DifyApiClient

## Overview

The refactored `DifyApiClient` is **100% backward compatible**. Existing code will continue to work without any changes. However, the new architecture enables advanced usage patterns.

## No Changes Required

### ✅ Existing Code Still Works

```csharp
// This code continues to work exactly as before
var options = new DifyApiClientOptions
{
    BaseUrl = "http://your-server/v1",
    ApiKey = "your-api-key"
};

using var client = new DifyApiClient(options);

// All existing methods work the same
var response = await client.SendChatMessageAsync(new ChatMessageRequest
{
    Query = "Hello",
    User = "user-123"
});

var conversations = await client.GetConversationsAsync("user-123");
var info = await client.GetApplicationInfoAsync();
```

## Optional: Leveraging the New Architecture

### Using Interfaces for Better Testing

```csharp
// Now you can use the interface for dependency injection
public class MyChatService
{
    private readonly IDifyApiClient _difyClient;
    
    public MyChatService(IDifyApiClient difyClient)
    {
        _difyClient = difyClient;
    }
    
    public async Task<string> GetAnswer(string question)
    {
        var response = await _difyClient.SendChatMessageAsync(
            new ChatMessageRequest { Query = question, User = "user" });
        return response.Answer;
    }
}

// In your DI container (e.g., Startup.cs, Program.cs)
services.AddSingleton<IDifyApiClient>(sp => 
{
    var options = new DifyApiClientOptions
    {
        BaseUrl = configuration["Dify:BaseUrl"],
        ApiKey = configuration["Dify:ApiKey"]
    };
    return new DifyApiClient(options);
});
```

### Improved Error Handling

The new architecture uses custom `DifyApiException` instead of generic `HttpRequestException`:

```csharp
// Before (still works, but less informative)
try
{
    await client.DeleteConversationAsync(conversationId, user);
}
catch (HttpRequestException ex)
{
    Console.WriteLine($"HTTP Error: {ex.Message}");
}

// After (recommended - more detailed information)
try
{
    await client.DeleteConversationAsync(conversationId, user);
}
catch (DifyApiException ex)
{
    Console.WriteLine($"Status Code: {ex.StatusCode}");
    Console.WriteLine($"Error Message: {ex.Message}");
    Console.WriteLine($"Response Body: {ex.ResponseBody}");
    
    if (ex.StatusCode == 404)
    {
        // Handle not found specifically
    }
}
```

### Unit Testing Made Easier

```csharp
// Mock the interface for testing
public class MyChatServiceTests
{
    [Fact]
    public async Task GetAnswer_ReturnsExpectedResponse()
    {
        // Arrange
        var mockClient = new Mock<IDifyApiClient>();
        mockClient
            .Setup(x => x.SendChatMessageAsync(
                It.IsAny<ChatMessageRequest>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ChatCompletionResponse 
            { 
                Answer = "Test Answer" 
            });
        
        var service = new MyChatService(mockClient.Object);
        
        // Act
        var result = await service.GetAnswer("Test question");
        
        // Assert
        Assert.Equal("Test Answer", result);
    }
}
```

## Breaking Changes

### None! 🎉

The refactoring was designed to be 100% backward compatible. All public APIs remain the same:
- Same method names
- Same parameter types
- Same return types
- Same constructor signatures

## What Changed Under the Hood

### Internal Architecture Improvements

1. **Service-Oriented Architecture**: The monolithic class now delegates to specialized services
2. **Better Error Handling**: Centralized in `BaseApiClient` with custom exceptions
3. **Code Reuse**: Common HTTP patterns extracted to reduce duplication
4. **Query String Building**: Cleaner with `QueryStringBuilder` utility

### For Library Maintainers

If you maintain this library, the new structure provides:

- **Easier Feature Addition**: Add new endpoints in focused service classes
- **Better Testing**: Test individual services independently
- **Cleaner Code Reviews**: Smaller, focused pull requests
- **Separation of Concerns**: Each service handles one domain

## Example: Adding a New Feature

### Before (Old Architecture)
```csharp
// Had to add to the 500+ line DifyApiClient class
public class DifyApiClient
{
    // ... 500+ lines ...
    
    public async Task<NewFeatureResponse> NewFeatureAsync(...)
    {
        // New code mixed with everything else
    }
}
```

### After (New Architecture)
```csharp
// Create a new focused service
public interface INewFeatureService
{
    Task<NewFeatureResponse> DoSomethingAsync(...);
}

public class NewFeatureService : BaseApiClient, INewFeatureService
{
    public NewFeatureService(HttpClient httpClient, JsonSerializerOptions jsonOptions)
        : base(httpClient, jsonOptions)
    {
    }
    
    public async Task<NewFeatureResponse> DoSomethingAsync(...)
    {
        return await GetAsync<NewFeatureResponse>("new-endpoint");
    }
}

// Add to DifyApiClient facade
public partial class DifyApiClient
{
    private readonly INewFeatureService _newFeatureService;
    
    // Constructor: initialize _newFeatureService
    
    // Delegate method
    public Task<NewFeatureResponse> DoSomethingAsync(...) =>
        _newFeatureService.DoSomethingAsync(...);
}
```

## Performance Impact

✅ **No Performance Impact**

- Same number of HTTP calls
- Same serialization/deserialization
- Minimal overhead from delegation (inlined by JIT)
- No lazy loading overhead (services created once at construction)

## Recommendations

1. **Update exception handling** to use `DifyApiException` for better error information
2. **Use `IDifyApiClient`** interface in your dependency injection setup
3. **Leverage mocking** in unit tests using the interface
4. **Continue using** the client exactly as before if you don't need these features

## Support

The refactoring maintains full backward compatibility. If you encounter any issues:

1. Check that you're using the same constructor and method calls
2. Update exception handling to catch `DifyApiException` (or keep catching the base `Exception`)
3. Verify NuGet package references are up to date

## Summary

✅ **No code changes required**  
✅ **Optional improvements available**  
✅ **Better architecture for future development**  
✅ **Improved testability and maintainability**
