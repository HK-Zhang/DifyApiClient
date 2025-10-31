# DifyApiClient Integration Tests

These tests are configured to run against a real Dify API endpoint.

## Configuration

### API Endpoint

The tests are configured to use: `http://localhost:8980/v1`

### API Key Configuration (User Secrets)

This project uses .NET User Secrets to store the API key securely. User secrets are stored outside the project directory and are never committed to source control.

**Set your API key:**

```powershell
cd tests/DifyApiClient.Tests
dotnet user-secrets set "DifyApiKey" "your-actual-api-key-here"
```

**View your secrets:**

```powershell
dotnet user-secrets list
```

**Remove a secret:**

```powershell
dotnet user-secrets remove "DifyApiKey"
```

**Clear all secrets:**

```powershell
dotnet user-secrets clear
```

## Running Tests

From the test project directory:

```powershell
dotnet test
```

To run a specific test:

```powershell
dotnet test --filter "FullyQualifiedName~SendChatMessageAsync_ReturnsValidResponse"
```

To run with verbose output:

```powershell
dotnet test --logger "console;verbosity=detailed"
```

## Important Notes

- These are **integration tests** that make real HTTP calls to the Dify API
- Test results depend on the actual state of your Dify instance
- Some tests may create data on the server (conversations, annotations, files)
- Ensure your API key has the necessary permissions
- The tests assume a chat application is configured on the Dify instance
- **User secrets are stored at:** `%APPDATA%\Microsoft\UserSecrets\dify-api-client-tests-12345\secrets.json`
