# Testing Guide

This document describes the testing infrastructure and how to run tests in the Workout Manager application.

## Test Projects

The solution contains four test projects:

1. **WorkoutManager.Api.Tests** - Integration tests for API endpoints
2. **WorkoutManager.BusinessLogic.Tests** - Unit tests for business logic
3. **WorkoutManager.Data.Tests** - Unit tests for data models
4. **WorkoutManager.E2E.Tests** - End-to-end tests with Playwright and Blazor component tests with bUnit

## Tech Stack

### Unit and Integration Testing
- **xUnit** - Testing framework
- **FluentAssertions** - Readable assertions
- **Bogus** - Test data generation
- **WebApplicationFactory** - API integration testing
- **Respawn** - Database cleanup between tests
- **Alba** - DSL for HTTP endpoint testing
- **Verify.Http** - Snapshot testing for API contracts

### E2E Testing
- **Microsoft Playwright for .NET** - Browser automation (Chromium)
- **bUnit** - Blazor component testing

### Code Coverage
- **coverlet.collector** - Code coverage collection
- **ReportGenerator** - HTML/lcov report generation

## Running Tests

### Run All Tests
```bash
dotnet test
```

### Run Specific Test Project
```bash
# API Integration Tests
dotnet test WorkoutManager.Api.Tests

# Business Logic Tests
dotnet test WorkoutManager.BusinessLogic.Tests

# Data Tests
dotnet test WorkoutManager.Data.Tests

# E2E Tests
dotnet test WorkoutManager.E2E.Tests
```

### Run Tests with Code Coverage
```bash
# Run tests and collect coverage
dotnet test --collect:"XPlat Code Coverage"

# Generate HTML report
reportgenerator `
  -reports:"**/coverage.cobertura.xml" `
  -targetdir:"TestResults/CoverageReport" `
  -reporttypes:"Html;lcov"

# Open the report
start TestResults/CoverageReport/index.html
```

### Run Tests with Filtering
```bash
# Run tests by category
dotnet test --filter Category=Unit

# Run tests by name pattern
dotnet test --filter FullyQualifiedName~ExerciseController

# Run tests by trait
dotnet test --filter Priority=1
```

## E2E Tests Setup

### Install Playwright Browsers
Before running E2E tests for the first time, install Chromium:

```bash
cd WorkoutManager.E2E.Tests
dotnet build
powershell -File bin/Debug/net9.0/playwright.ps1 install chromium
```

### Running E2E Tests
```bash
# Make sure the application is running
dotnet run --project WorkoutManager.Web

# In another terminal, run E2E tests
dotnet test WorkoutManager.E2E.Tests
```

## Test Infrastructure

### API Integration Tests
Base class: `BaseIntegrationTest`
- Uses `WebApplicationFactory` for in-memory testing
- Provides `Alba` host for HTTP testing
- Includes test data generators using Bogus

Example:
```csharp
public class ExercisesControllerTests : BaseIntegrationTest
{
    public ExercisesControllerTests(IntegrationTestWebAppFactory factory) 
        : base(factory) { }

    [Fact]
    public async Task GetExercises_ReturnsSuccessStatusCode()
    {
        var response = await Host.Scenario(cfg =>
        {
            cfg.Get.Url("/api/exercises");
            cfg.StatusCodeShouldBeOk();
        });
    }
}
```

### Business Logic Tests
Base class: `BaseTest`
- Test data builders using Bogus
- Focused on business rules validation

### E2E Tests with Playwright
Base class: `BasePlaywrightTest`
- Page Object Model pattern
- Chromium browser configuration
- Screenshot on failure
- Video recording on failure

Example:
```csharp
[Test]
public async Task LoginPage_ShouldBeAccessible()
{
    var loginPage = new LoginPage(Page, BaseUrl);
    await loginPage.NavigateAsync();
    var isVisible = await loginPage.IsLoginFormVisibleAsync();
    isVisible.Should().BeTrue();
}
```

### Blazor Component Tests with bUnit
```csharp
[Test]
public void Component_ShouldRender()
{
    var context = new TestContext();
    var cut = context.RenderComponent<YourComponent>();
    cut.Markup.Should().Contain("expected content");
}
```

## Continuous Integration

### GitHub Actions Workflow
The test pipeline automatically:
1. Runs all unit and integration tests
2. Runs E2E tests on Chromium
3. Generates code coverage reports
4. Uploads coverage to reporting services

## Best Practices

1. **Arrange-Act-Assert** pattern for test structure
2. **One assertion per test** when possible
3. **Meaningful test names** that describe the scenario
4. **Use test data builders** (Bogus) instead of manual data creation
5. **Clean up resources** after tests (IAsyncLifetime)
6. **Isolate tests** - each test should be independent
7. **Use Page Object Model** for E2E tests
8. **Mock external dependencies** in unit tests
9. **Test behavior, not implementation**
10. **Keep tests fast** - use in-memory databases for integration tests

## Troubleshooting

### Playwright Issues
If Playwright tests fail:
```bash
# Reinstall browsers
cd WorkoutManager.E2E.Tests
powershell -File bin/Debug/net9.0/playwright.ps1 install chromium --force
```

### Code Coverage Not Generated
Ensure coverlet.collector is referenced in all test projects:
```xml
<PackageReference Include="coverlet.collector" Version="6.0.4" />
```

### Tests Timing Out
Increase timeout in xunit.runner.json:
```json
{
  "methodDisplay": "method",
  "methodDisplayOptions": "all",
  "diagnosticMessages": true,
  "maxParallelThreads": -1,
  "longRunningTestSeconds": 60
}
```

## Further Reading

- [xUnit Documentation](https://xunit.net/)
- [FluentAssertions](https://fluentassertions.com/)
- [Playwright for .NET](https://playwright.dev/dotnet/)
- [bUnit Documentation](https://bunit.dev/)
- [Alba Documentation](https://jasperfx.github.io/alba/)



