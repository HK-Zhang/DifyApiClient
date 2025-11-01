using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DifyApiClient.Extensions;

/// <summary>
/// Extension methods for configuring DifyApiClient in dependency injection
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds DifyApiClient to the service collection using IHttpClientFactory
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configure">Configuration action for DifyApiClientOptions</param>
    /// <returns>The service collection for chaining</returns>
    public static IHttpClientBuilder AddDifyApiClient(
        this IServiceCollection services,
        Action<DifyApiClientOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        var options = new DifyApiClientOptions
        {
            BaseUrl = "placeholder",
            ApiKey = "placeholder"
        };
        configure(options);

        services.AddSingleton(options);

        var httpClientBuilder = services.AddHttpClient<IDifyApiClient, DifyApiClient>()
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler());

        services.AddTransient<IDifyApiClient>(serviceProvider =>
        {
            var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient(typeof(DifyApiClient).FullName ?? "DifyApiClient");
            var clientOptions = serviceProvider.GetRequiredService<DifyApiClientOptions>();
            var logger = serviceProvider.GetService<ILogger<DifyApiClient>>();
            
            return new DifyApiClient(clientOptions, httpClient, disposeHttpClient: false, logger: logger);
        });

        return httpClientBuilder;
    }

    /// <summary>
    /// Adds DifyApiClient to the service collection using IHttpClientFactory with options instance
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="options">Pre-configured DifyApiClientOptions instance</param>
    /// <returns>The service collection for chaining</returns>
    public static IHttpClientBuilder AddDifyApiClient(
        this IServiceCollection services,
        DifyApiClientOptions options)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(options);

        services.AddSingleton(options);

        var httpClientBuilder = services.AddHttpClient<IDifyApiClient, DifyApiClient>()
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler());

        services.AddTransient<IDifyApiClient>(serviceProvider =>
        {
            var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient(typeof(DifyApiClient).FullName ?? "DifyApiClient");
            var clientOptions = serviceProvider.GetRequiredService<DifyApiClientOptions>();
            var logger = serviceProvider.GetService<ILogger<DifyApiClient>>();
            
            return new DifyApiClient(clientOptions, httpClient, disposeHttpClient: false, logger: logger);
        });

        return httpClientBuilder;
    }

    /// <summary>
    /// Adds DifyApiClient to the service collection using IHttpClientFactory with resilience policies
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configure">Configuration action for DifyApiClientOptions</param>
    /// <param name="configureHttpClient">Optional configuration for HttpClient resilience</param>
    /// <returns>The service collection for chaining</returns>
    public static IHttpClientBuilder AddDifyApiClientWithResilience(
        this IServiceCollection services,
        Action<DifyApiClientOptions> configure,
        Action<IHttpClientBuilder>? configureHttpClient = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        var httpClientBuilder = services.AddDifyApiClient(configure);

        // Apply standard resilience policies
        httpClientBuilder.AddStandardResiliencePolicies();

        // Allow custom resilience configuration
        configureHttpClient?.Invoke(httpClientBuilder);

        return httpClientBuilder;
    }
}
