# Smart CV Filter API - Unit Tests

This directory contains comprehensive unit tests for the Smart CV Filter API backend, following the 80:20 rule to achieve high code coverage while focusing on critical business logic.

## Test Structure

```
tests/
├── SmartCVFilter.API.Tests/
│   ├── Controllers/           # API Controller tests
│   ├── Services/              # Business logic service tests
│   ├── Integration/           # Integration tests
│   ├── TestBase/              # Test utilities and base classes
│   ├── appsettings.Test.json  # Test configuration
│   └── SmartCVFilter.API.Tests.csproj
├── run-tests.ps1             # PowerShell test runner
├── run-tests.sh              # Bash test runner
└── README.md                 # This file
```

## Test Coverage (80:20 Rule)

### High Priority Tests (80% of effort)

- **Authentication Service** - Critical security logic
- **Job Post Service** - Core business functionality
- **Applicant Service** - Main business processes
- **API Controllers** - Public interface validation
- **Gemini AI Service** - AI integration logic

### Lower Priority Tests (20% of effort)

- **Utility Services** - Supporting functionality
- **Edge Cases** - Error scenarios and boundary conditions

## Test Categories

### 1. Unit Tests

- **Service Layer Tests**: Test business logic in isolation
- **Controller Tests**: Test API endpoints with mocked dependencies
- **Model Tests**: Test data validation and mapping

### 2. Integration Tests

- **API Integration**: Test complete request/response cycles
- **Database Integration**: Test with in-memory database
- **Authentication Flow**: Test end-to-end auth scenarios

## Running Tests

### Prerequisites

- .NET 8 SDK
- Test project dependencies installed

### Quick Start

```bash
# PowerShell (Windows)
cd tests
.\run-tests.ps1

# Bash (Linux/macOS)
cd tests
chmod +x run-tests.sh
./run-tests.sh
```

### Manual Test Execution

```bash
cd tests/SmartCVFilter.API.Tests
dotnet test
```

### With Coverage Report

```bash
cd tests/SmartCVFilter.API.Tests
dotnet test --collect:"XPlat Code Coverage"
```

## Test Configuration

### Test Database

- Uses **In-Memory Database** for fast, isolated tests
- Each test gets a fresh database instance
- No external database dependencies

### Mocking Strategy

- **Services**: Mocked using Moq framework
- **HTTP Clients**: Mocked for external API calls
- **Logging**: Mocked to avoid console noise
- **Configuration**: In-memory configuration for tests

### Test Data

- **TestBase Class**: Provides helper methods for creating test data
- **Sample Data**: Realistic test data for comprehensive testing
- **Isolation**: Each test is completely isolated

## Test Examples

### Service Test Example

```csharp
[Fact]
public async Task CreateJobPostAsync_WithValidData_ShouldReturnJobPostResponse()
{
    // Arrange
    var user = await CreateTestUserAsync();
    var request = new CreateJobPostRequest { /* ... */ };

    // Act
    var result = await _jobPostService.CreateJobPostAsync(request, user.Id);

    // Assert
    result.Should().NotBeNull();
    result.Title.Should().Be(request.Title);
}
```

### Controller Test Example

```csharp
[Fact]
public async Task Register_WithValidData_ShouldReturnOkResult()
{
    // Arrange
    var request = new RegisterRequest { /* ... */ };
    var authResponse = new AuthResponse { /* ... */ };
    _mockAuthService.Setup(x => x.RegisterAsync(request)).ReturnsAsync(authResponse);

    // Act
    var result = await _controller.Register(request);

    // Assert
    result.Result.Should().BeOfType<OkObjectResult>();
}
```

## Test Utilities

### TestBase Class

- **Database Setup**: Configures in-memory database
- **User Creation**: Helper methods for creating test users
- **Data Creation**: Helper methods for creating test entities
- **Cleanup**: Automatic cleanup after each test

### Mock Helpers

- **Logger Mocks**: Pre-configured logger mocks
- **Service Mocks**: Common service mocking patterns
- **HTTP Client Mocks**: Mock external API calls

## Coverage Goals

### Target Coverage: 80%

- **Critical Paths**: 95%+ coverage
- **Business Logic**: 90%+ coverage
- **API Endpoints**: 85%+ coverage
- **Error Handling**: 80%+ coverage

### Coverage Exclusions

- **Startup Code**: Program.cs, configuration
- **Generated Code**: Auto-generated files
- **Third-party Code**: External library code

## Best Practices

### Test Naming

- **Format**: `MethodName_Scenario_ExpectedResult`
- **Examples**:
  - `CreateJobPostAsync_WithValidData_ShouldReturnJobPostResponse`
  - `Login_WithInvalidCredentials_ShouldReturnUnauthorized`

### Test Structure

- **Arrange**: Set up test data and mocks
- **Act**: Execute the method under test
- **Assert**: Verify the expected outcome

### Assertions

- **FluentAssertions**: Use for readable assertions
- **Specific Checks**: Test specific properties, not just null checks
- **Error Messages**: Verify error messages and status codes

## Continuous Integration

### GitHub Actions

```yaml
- name: Run Tests
  run: |
    cd tests/SmartCVFilter.API.Tests
    dotnet test --collect:"XPlat Code Coverage"
```

### Local Development

- **Pre-commit**: Run tests before committing
- **IDE Integration**: Run tests in Visual Studio/VS Code
- **Watch Mode**: `dotnet watch test` for continuous testing

## Troubleshooting

### Common Issues

1. **Test Database Issues**

   - Ensure each test uses a fresh database
   - Check for proper cleanup in test teardown

2. **Mock Configuration**

   - Verify mock setups match actual method calls
   - Check for proper mock verification

3. **Async/Await**

   - Ensure all async methods are properly awaited
   - Use `async Task` for async test methods

4. **Test Isolation**
   - Each test should be independent
   - No shared state between tests

### Debug Tips

- Use `Debugger.Break()` in tests for debugging
- Check test output for detailed error messages
- Verify mock setups and method calls

## Performance Considerations

### Test Speed

- **In-Memory Database**: Fast database operations
- **Mocked Dependencies**: No external service calls
- **Parallel Execution**: Tests run in parallel by default

### Memory Usage

- **Dispose Pattern**: Proper cleanup of resources
- **Test Isolation**: Fresh instances for each test
- **Mock Cleanup**: Automatic cleanup of mocks

## Future Enhancements

### Planned Improvements

- **Performance Tests**: Load testing for critical endpoints
- **Contract Tests**: API contract validation
- **Security Tests**: Authentication and authorization testing
- **End-to-End Tests**: Complete user journey testing

### Test Data Management

- **Test Data Builders**: Fluent API for creating test data
- **Data Factories**: Centralized test data creation
- **Scenario Data**: Predefined test scenarios

This test suite provides comprehensive coverage of the Smart CV Filter API, ensuring reliability and maintainability while following industry best practices.
