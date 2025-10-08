# 10xWorkoutManager

[![.NET](https://img.shields.io/badge/.NET-9.0-blue.svg)](https://dotnet.microsoft.com/)
[![Blazor](https://img.shields.io/badge/Blazor-WebAssembly-green.svg)](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

## 📋 Table of Contents
- [Project Description](#-project-description)
- [Tech Stack](#-tech-stack)
- [Getting Started Locally](#-getting-started-locally)
- [Available Scripts](#-available-scripts)
- [Project Scope](#-project-scope)
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
- **DigitalOcean**: For hosting the application via Docker image

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

# Run tests (when implemented)
dotnet test

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
