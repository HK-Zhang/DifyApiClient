using DifyApiClient;
using DifyApiClient.Models;
using Microsoft.Extensions.Configuration;

namespace DifyApiClient.Samples;

/// <summary>
/// Sample application demonstrating Dify API Client usage
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== Dify API Client Sample ===\n");

        // Load configuration from user secrets
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .Build();

        // Configure the client
        var options = new DifyApiClientOptions
        {
            BaseUrl = configuration["DifyApi:BaseUrl"] ?? throw new InvalidOperationException("DifyApi:BaseUrl not found in user secrets"),
            ApiKey = configuration["DifyApi:ApiKey"] ?? throw new InvalidOperationException("DifyApi:ApiKey not found in user secrets")
        };

        using var client = new DifyApiClient(options);

        // Menu
        while (true)
        {
            Console.WriteLine("\nSelect an operation:");
            Console.WriteLine("1. Send Chat Message (Blocking)");
            Console.WriteLine("2. Send Chat Message (Streaming)");
            Console.WriteLine("3. Get Application Info");
            Console.WriteLine("4. List Conversations");
            Console.WriteLine("5. Manage Annotations");
            Console.WriteLine("6. Upload File");
            Console.WriteLine("0. Exit");
            Console.Write("\nYour choice: ");

            var choice = Console.ReadLine();

            try
            {
                switch (choice)
                {
                    case "1":
                        await SendChatMessageAsync(client);
                        break;
                    case "2":
                        await SendChatMessageStreamAsync(client);
                        break;
                    case "3":
                        await GetApplicationInfoAsync(client);
                        break;
                    case "4":
                        await ListConversationsAsync(client);
                        break;
                    case "5":
                        await ManageAnnotationsAsync(client);
                        break;
                    case "6":
                        await UploadFileAsync(client);
                        break;
                    case "0":
                        Console.WriteLine("Goodbye!");
                        return;
                    default:
                        Console.WriteLine("Invalid choice!");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }

    static async Task SendChatMessageAsync(DifyApiClient client)
    {
        Console.Write("Enter your message: ");
        var query = Console.ReadLine() ?? string.Empty;

        Console.Write("Enter user ID: ");
        var userId = Console.ReadLine() ?? "demo-user";

        var request = new ChatMessageRequest
        {
            Query = query,
            User = userId
        };

        Console.WriteLine("\nSending message...");
        var response = await client.SendChatMessageAsync(request);

        Console.WriteLine($"\n--- Response ---");
        Console.WriteLine($"Message ID: {response.MessageId}");
        Console.WriteLine($"Conversation ID: {response.ConversationId}");
        Console.WriteLine($"Answer: {response.Answer}");
        
        if (response.Metadata?.Usage != null)
        {
            Console.WriteLine($"\nTokens Used: {response.Metadata.Usage.TotalTokens}");
        }
    }

    static async Task SendChatMessageStreamAsync(DifyApiClient client)
    {
        Console.Write("Enter your message: ");
        var query = Console.ReadLine() ?? string.Empty;

        Console.Write("Enter user ID: ");
        var userId = Console.ReadLine() ?? "demo-user";

        var request = new ChatMessageRequest
        {
            Query = query,
            User = userId
        };

        Console.WriteLine("\nStreaming response:\n");
        Console.Write("Assistant: ");

        await foreach (var chunk in client.SendChatMessageStreamAsync(request))
        {
            if (chunk.Event == "message" && !string.IsNullOrEmpty(chunk.Answer))
            {
                Console.Write(chunk.Answer);
            }
            else if (chunk.Event == "message_end")
            {
                Console.WriteLine("\n\n[Stream Complete]");
                break;
            }
        }
    }

    static async Task GetApplicationInfoAsync(DifyApiClient client)
    {
        Console.WriteLine("\nFetching application information...");

        var info = await client.GetApplicationInfoAsync();
        Console.WriteLine($"\n--- Application Info ---");
        Console.WriteLine($"Name: {info.Name}");
        Console.WriteLine($"Description: {info.Description}");
        Console.WriteLine($"Mode: {info.Mode}");
        Console.WriteLine($"Author: {info.AuthorName}");
        
        if (info.Tags != null && info.Tags.Any())
        {
            Console.WriteLine($"Tags: {string.Join(", ", info.Tags)}");
        }

        var parameters = await client.GetApplicationParametersAsync();
        Console.WriteLine($"\n--- Parameters ---");
        Console.WriteLine($"Opening Statement: {parameters.OpeningStatement}");
        Console.WriteLine($"Speech to Text: {parameters.SpeechToText?.Enabled}");
        Console.WriteLine($"Text to Speech: {parameters.TextToSpeech?.Enabled}");
    }

    static async Task ListConversationsAsync(DifyApiClient client)
    {
        Console.Write("Enter user ID: ");
        var userId = Console.ReadLine() ?? "demo-user";

        Console.WriteLine("\nFetching conversations...");
        var conversations = await client.GetConversationsAsync(userId);

        Console.WriteLine($"\n--- Conversations (Page {conversations.Page}) ---");
        
        if (conversations.Data == null || !conversations.Data.Any())
        {
            Console.WriteLine("No conversations found.");
            return;
        }

        foreach (var conv in conversations.Data)
        {
            Console.WriteLine($"\nID: {conv.Id}");
            Console.WriteLine($"Name: {conv.Name}");
            Console.WriteLine($"Status: {conv.Status}");
            Console.WriteLine($"Created: {DateTimeOffset.FromUnixTimeSeconds(conv.CreatedAt):yyyy-MM-dd HH:mm:ss}");
        }

        Console.WriteLine($"\nHas More: {conversations.HasMore}");
    }

    static async Task ManageAnnotationsAsync(DifyApiClient client)
    {
        Console.WriteLine("\nAnnotation Operations:");
        Console.WriteLine("1. List Annotations");
        Console.WriteLine("2. Create Annotation");
        Console.WriteLine("3. Delete Annotation");
        Console.Write("Choice: ");

        var choice = Console.ReadLine();

        switch (choice)
        {
            case "1":
                var annotations = await client.GetAnnotationsAsync();
                Console.WriteLine($"\n--- Annotations (Total: {annotations.Total}) ---");
                
                if (annotations.Data != null)
                {
                    foreach (var ann in annotations.Data)
                    {
                        Console.WriteLine($"\nID: {ann.Id}");
                        Console.WriteLine($"Q: {ann.Question}");
                        Console.WriteLine($"A: {ann.Answer}");
                        Console.WriteLine($"Hits: {ann.HitCount}");
                    }
                }
                break;

            case "2":
                Console.Write("Enter question: ");
                var question = Console.ReadLine() ?? string.Empty;

                Console.Write("Enter answer: ");
                var answer = Console.ReadLine() ?? string.Empty;

                var newAnnotation = await client.CreateAnnotationAsync(new AnnotationRequest
                {
                    Question = question,
                    Answer = answer
                });

                Console.WriteLine($"\nCreated annotation: {newAnnotation.Id}");
                break;

            case "3":
                Console.Write("Enter annotation ID: ");
                var annotationId = Console.ReadLine() ?? string.Empty;

                await client.DeleteAnnotationAsync(annotationId);
                Console.WriteLine("Annotation deleted successfully!");
                break;

            default:
                Console.WriteLine("Invalid choice!");
                break;
        }
    }

    static async Task UploadFileAsync(DifyApiClient client)
    {
        Console.Write("Enter file path: ");
        var filePath = Console.ReadLine() ?? string.Empty;

        if (!File.Exists(filePath))
        {
            Console.WriteLine("File not found!");
            return;
        }

        Console.Write("Enter user ID: ");
        var userId = Console.ReadLine() ?? "demo-user";

        Console.WriteLine("\nUploading file...");

        using var fileStream = File.OpenRead(filePath);
        var fileInfo = await client.UploadFileAsync(
            fileStream,
            Path.GetFileName(filePath),
            userId
        );

        Console.WriteLine($"\n--- Upload Success ---");
        Console.WriteLine($"File ID: {fileInfo.Id}");
        Console.WriteLine($"Name: {fileInfo.Name}");
        Console.WriteLine($"Size: {fileInfo.Size} bytes");
        Console.WriteLine($"Type: {fileInfo.MimeType}");
    }
}
