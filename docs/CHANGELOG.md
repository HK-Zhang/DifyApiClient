# Changelog

All notable changes to DifyApiClient will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- **Production-Ready Features** - Added comprehensive non-functional features for enterprise use
- **ConfigureAwait(false)** - All async calls now use ConfigureAwait(false) to prevent deadlocks
- **IHttpClientFactory Support** - Full integration with IHttpClientFactory for better HttpClient lifecycle management
- **Dependency Injection Extensions** - `AddDifyApiClient()` and `AddDifyApiClientWithResilience()` extension methods
- **Polly Resilience Policies** - Integrated retry, circuit breaker, and timeout policies
- **XML Documentation Generation** - Enabled XML documentation file for better IntelliSense
- **User-Agent Header** - Automatic User-Agent header with library name and version
- **Deterministic Builds** - Enabled deterministic and CI builds for reproducibility
- **Comprehensive Documentation** - Added DEPENDENCY_INJECTION.md and RESILIENCE.md guides

### Changed
- **Multi-Targeting** - Now targets .NET 8.0 and .NET 9.0 (was .NET 9.0 only)
- **Package Dependencies** - Added Microsoft.Extensions.Http, Polly, and related packages
- **README** - Updated with dependency injection quick start and new feature highlights

### Technical Details
- Added 7 high-priority production-ready features
- Created 2 new comprehensive documentation files (1100+ lines)
- Zero breaking changes - fully backward compatible
- All async methods now use ConfigureAwait(false)
- Standard resilience policies: 3 retries with exponential backoff, circuit breaker after 5 failures 

## [1.0.0] - 2025-10-31

### Added
- Initial release of DifyApiClient
- Complete implementation of Dify Chat Application API
- Chat operations with blocking and streaming support
- File upload functionality
- Conversation management (list, get messages, delete, rename)
- Message feedback submission
- Audio operations (speech-to-text, text-to-audio)
- Application information retrieval (info, parameters, meta, site)
- Annotations management (CRUD operations)
- Feedback retrieval
- Comprehensive unit test coverage
- Async/await pattern throughout
- IAsyncEnumerable support for streaming responses
- Cancellation token support
- Strong typing with dedicated model classes
- Proper error handling with DifyApiException
- IDisposable implementation
- Nullable reference types enabled
- XML documentation for public APIs

### Technical Details
- Target Framework: .NET 9.0
- Dependencies: None (runtime)
- Package Size: ~38 KB
- Includes debug symbols (snupkg)

[Unreleased]: https://github.com/HK-Zhang/DifyApiClient/compare/v1.0.0...HEAD
[1.0.0]: https://github.com/HK-Zhang/DifyApiClient/releases/tag/v1.0.0
