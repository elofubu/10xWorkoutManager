Backend + Frontend
- .NET Blazor WebAssembly:
  - allows you to create fast and efficient pages
  - has static typing
CSS:
- BlazorBootstrap - component library and css tools
Database:
- Supabase:
  - Provides a PosgreSQL database
  - Provides SDKs in many languages ​​that will serve as Backend-as-a-Service
  - It is an open source solution that can be hosted locally or on your own server
  - Has built-in user authentication

Testing:
1) Unit and integration tests:
  - xUnit - testing framework (standard for .NET)
  - FluentAssertions - readable and extensive assertions
  - Bogus - generating realistic test data
  - WebApplicationFactory - API integration tests
  - Respawn - fast database reset between tests
  - Alba - DSL for testing HTTP endpoints
  - Verify.Http - snapshot testing for API contracts
  - FluentValidation.TestHelper - validator tests

2) E2E (End-to-End) tests:
  - Microsoft Playwright for .NET - browser automation
    - Support for Chromium, Firefox, WebKit
    - Headless mode for CI/CD
    - Dedicated to testing .NET and Blazor WASM applications
  - bUnit - testing Blazor components (rendering, state, events)

3) Architecture and quality tests:
  - NetArchTest - verification of layered architecture rules
  - Stryker.NET - mutation testing (detection of weak tests)
  - Security Code Scan - static security analysis (SAST)
  - SonarQube Community - code quality and security gates

4) Performance tests:
  - k6 - load and performance tests (main tool)
  - NBomber - an alternative in C# for performance testing

5) Code coverage and reports:
  - coverlet.collector - collecting code coverage metrics
  - ReportGenerator - generating HTML/lcov reports

CI/CD and Hosting:
- Github Actions for creating CI/CD pipelines
  - Automatic running of tests (unit, integration, e2e)
  - Generating code coverage reports
  - Test matrix for various browsers (Playwright)
- DigitalOcean for hosting applications via a docker image