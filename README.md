# 10xWorkoutManager - test PR

[![.NET](https://img.shields.io/badge/.NET-9.0-blue.svg)](https://dotnet.microsoft.com/)
[![Blazor](https://img.shields.io/badge/Blazor-WebAssembly-green.svg)](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor)
[![Tests](https://img.shields.io/badge/Tests-xUnit%20%7C%20Playwright-green.svg)](https://github.com/xunit/xunit)
[![Coverage](https://img.shields.io/badge/Coverage-80%25%20target-brightgreen.svg)](#-testing-strategy)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

## 📋 Table of Contents
- [Project Description](#-project-description)
- [Tech Stack](#-tech-stack)
- [Getting Started Locally](#-getting-started-locally)
- [Available Scripts](#-available-scripts)
- [Project Scope](#-project-scope)
- [Testing Strategy](#-testing-strategy)
- [Project Status](#-project-status)
- [License](#-license)

## 🎯 Project Description

10xWorkoutManager is a web application designed to solve the problem of inefficient workout plan management and progress tracking. The app enables users to create, edit, and manage workout plans, conduct training sessions, and browse their performance history. The goal is to replace inconvenient methods like paper notebooks or spreadsheets with an intuitive, centralized digital tool that supports users in achieving their training goals through easy access to previous training data.

### Key Features
- **User Account Management**: Registration, login, password reset, and account deletion
- **Workout Plan Management**: Create, view, edit, and delete plans with drag-and-drop functionality
- **Exercise Management**: Access to a predefined exercise database, search functionality, and ability to add custom exercises
- **Training Sessions**: Start workouts, save sets with weight/reps, skip exercises, and add notes
- **Training History**: Chronological list of all saved sessions with detailed summaries

## 🛠 Tech Stack

### Frontend & Backend
- **.NET Blazor WebAssembly**: For creating fast and efficient web pages with static typing
- **Blazor.Bootstrap**: Component library and CSS tools for modern UI

### Database
- **Supabase**: Provides PostgreSQL database, multi-language SDK as Backend-as-a-Service, built-in user authentication, open-source solution that can be hosted locally or on your own server

### CI/CD & Hosting
- **GitHub Actions**: For creating CI/CD pipelines
- **Azure Static Web Apps**: For hosting the Blazor WASM frontend with global distribution and CI/CD integration.
- **Azure App Service**: For hosting the .NET Web API backend, providing a scalable and managed environment.

### Testing
- **xUnit**: Modern testing framework for .NET unit and integration tests
- **FluentAssertions**: Readable and expressive assertions
- **Bogus**: Realistic test data generation
- **WebApplicationFactory**: Integration testing for API endpoints
- **Respawn**: Fast database cleanup between tests
- **Playwright for .NET**: End-to-end browser automation testing
- **bUnit**: Component testing for Blazor
- **Alba**: DSL for HTTP API testing
- **Verify.Http**: Snapshot testing for API contracts
- **NetArchTest**: Architecture rules validation
- **k6**: Performance and load testing
- **coverlet.collector**: Code coverage metrics

### Dependencies
- Microsoft.AspNetCore.Components.WebAssembly (9.0.9)
- Blazor.Bootstrap (3.4.0)

## 🚀 Getting Started Locally

### Prerequisites
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [Visual Studio Code](https://code.visualstudio.com/) with C# extension
- Supabase account for database setup

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/your-username/10x-workout-manager.git
   cd 10x-workout-manager
   ```

2. **Restore NuGet packages**
   ```bash
   dotnet restore
   ```

3. **Set up Supabase**
   - Create a new project on [Supabase](https://supabase.com)
   - Configure your database schema according to the project requirements
   - Set up authentication settings
   - Update connection strings in `appsettings.json` or environment variables

4. **Run the application**
   ```bash
   dotnet run
   ```

5. **Open your browser**
   Navigate to `https://localhost:5001` (or the port specified in your launch settings)

## 📜 Available Scripts

```bash
# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run the application in development mode
dotnet run

# Run all tests with code coverage
dotnet test --collect:"XPlat Code Coverage"

# Run only unit tests
dotnet test --filter Category=Unit

# Run only integration tests
dotnet test --filter Category=Integration

# Run E2E tests (Playwright)
cd tests/WorkoutManager.Web.E2E
playwright install
dotnet test

# Run performance tests (k6)
k6 run performance/api-load-test.js

# Generate coverage report
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:**/coverage.cobertura.xml -targetdir:coverage-report

# Publish the application
dotnet publish -c Release

# Create Docker image (when Docker setup is implemented)
docker build -t 10x-workout-manager .
```

## 📋 Project Scope

### MVP Features (Included)
- ✅ User authentication system (registration/login)
- ✅ Complete workout plan management cycle (create, edit, delete)
- ✅ Exercise management within plans
- ✅ Predefined exercise database with ability to add custom exercises
- ✅ Interactive workout session module
- ✅ Display of results from last exercise execution during training
- ✅ Simple history of completed workouts
- ✅ Web-only application
- ✅ Use of kilograms (kg) as the only weight unit

### Future Enhancements (Not in MVP)
- ❌ Mobile applications (iOS, Android)
- ❌ Advanced progress visualization (charts, statistics)
- ❌ Social features (plan sharing, result comparison)
- ❌ Integration with other devices (smartwatches)
- ❌ Addition of secondary activities (cardio, stretching) to plans
- ❌ Predefined workout plans
- ❌ Display of results from the most recently executed entire plan

## 🧪 Testing Strategy

The project implements comprehensive testing at multiple levels to ensure code quality, reliability, and maintainability:

### Test Coverage
- **Unit Tests**: Business logic validation, validators, service layer (~80% coverage target)
- **Integration Tests**: API endpoints with real database connections using WebApplicationFactory
- **Component Tests**: Blazor components tested with bUnit
- **E2E Tests**: Full user workflows automated with Playwright for .NET
- **Architecture Tests**: Enforcing layered architecture rules with NetArchTest
- **Performance Tests**: Load testing critical endpoints with k6

### Running Tests
All tests are automated and run in CI/CD pipeline. See [Available Scripts](#-available-scripts) for commands to run different test suites locally.

### Test Projects Structure
```
tests/
├── WorkoutManager.BusinessLogic.Tests/    # Unit tests
├── WorkoutManager.Api.IntegrationTests/   # API integration tests
├── WorkoutManager.Web.ComponentTests/     # Blazor component tests
├── WorkoutManager.Web.E2E/                # End-to-end tests
└── WorkoutManager.ArchitectureTests/      # Architecture validation
```

For detailed testing documentation, see [`.cursor/test-plan.md`](.cursor/test-plan.md).

## 📊 Project Status

**Status**: MVP Development

This project is currently in the MVP (Minimum Viable Product) development phase. The core functionality for workout planning, execution, and tracking has been implemented. The application is ready for initial user testing and feedback collection.

### Success Metrics
- **Primary KPI**: Average number of saved workout sessions per active user monthly
- **Target**: At least 4 saved workouts per active user monthly (indicating weekly usage)
- **Additional Metrics**: User retention rate, number of created plans per user, session completion rate

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

**10xWorkoutManager** - Transform your workout management experience with modern web technology.
