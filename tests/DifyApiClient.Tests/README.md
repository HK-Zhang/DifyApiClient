# DifyApiClient Integration Tests

These tests are configured to run against a real Dify API endpoint.

## Configuration

### API Endpoint
The tests are configured to use: `http://osl4243:8980/v1`

### API Key
Set the `DIFY_API_KEY` environment variable with your actual Dify API key before running the tests.

**PowerShell:**
```powershell
$env:DIFY_API_KEY = "app-your-actual-api-key"
dotnet test
```

**CMD:**
```cmd
set DIFY_API_KEY=app-your-actual-api-key
dotnet test
```

**Bash:**
```bash
export DIFY_API_KEY=app-your-actual-api-key
dotnet test
```

If no environment variable is set, the tests will use `app-your-api-key-here` as a placeholder (which will likely fail).

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
