# Changelog

All notable changes to DifyApiClient will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- 

### Changed
- 

### Deprecated
- 

### Removed
- 

### Fixed
- 

### Security
- 

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
