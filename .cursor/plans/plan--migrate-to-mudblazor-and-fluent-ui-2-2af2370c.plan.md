<!-- 2af2370c-5c85-4913-83f9-e6c249f30e5d 9e5fcc41-866c-46b4-b5d3-4af83115baa9 -->
# Plan: Migrate to MudBlazor and Fluent UI 2

This plan details the steps to transition the `WorkoutManager.Web` project from its current Bootstrap-based implementation to using the MudBlazor component library, aiming for a modern aesthetic inspired by Fluent UI 2.

### 1. Project Setup & Configuration

First, I'll set up the project dependencies and services required by MudBlazor.

-   **Add NuGet Package**: I will add the `MudBlazor` NuGet package to the `WorkoutManager.Web.csproj` file.
-   **Register Services**: In `WorkoutManager.Web/Program.cs`, I will register the necessary MudBlazor services by adding `builder.Services.AddMudServices();`.
-   **Global Usings**: I will update `WorkoutManager.Web/_Imports.razor` to include `@using MudBlazor` and remove the existing `@using BlazorBootstrap;`.

### 2. Update Static Assets and Styles

Next, I will swap out the Bootstrap assets in favor of MudBlazor's stylesheets and scripts in `WorkoutManager.Web/wwwroot/index.html`.

-   **Remove Bootstrap**: I will remove the `<link>` tags for `bootstrap.min.css`, `bootstrap-icons.min.css`, and `blazor.bootstrap.css`. I will also remove the `<script>` tags for `bootstrap.bundle.min.js` and `blazor.bootstrap.js`.
-   **Add MudBlazor Assets**: I will add the recommended fonts and CSS from MudBlazor, along with the necessary JavaScript file.

I will also clean up `WorkoutManager.Web/wwwroot/css/app.css` by removing styles that were specifically for overriding or complementing Bootstrap, which will no longer be needed.

### 3. Refactor Application Layout

The core layout of the application needs to be rebuilt using MudBlazor components.

-   **Main Layout**: I'll refactor `WorkoutManager.Web/Layout/MainLayout.razor` to use `MudLayout`, `MudAppBar`, `MudDrawer`, and `MudMainContent`. This will also involve adding a `MudThemeProvider` to control the application's theme and a `MudDialogProvider` and `MudSnackbarProvider` for interactive UI elements.
-   **Navigation Menu**: I'll update `WorkoutManager.Web/Layout/NavMenu.razor` to use MudBlazor's navigation components like `MudNavMenu` and `MudNavLink` for a consistent look and feel.

### Token-Level Modifications and Best Practices

-   **Readability**: By using MudBlazor's component-based approach, styling and behavior are defined declaratively in the Razor markup (e.g., `<MudButton Variant="Variant.Filled" Color="Color.Primary">Click Me</MudButton>`). This is more readable and maintainable than scattering custom CSS classes.
-   **Accessibility**: MudBlazor components are designed with accessibility in mind. By using them correctly, we inherit features like proper ARIA roles and keyboard navigation support. The `MudThemeProvider` will be used to ensure the color scheme has sufficient contrast.
-   **Future Work**: After this foundational work, individual pages and components will need to be migrated from standard HTML with Bootstrap classes to their corresponding MudBlazor components (e.g., `<table>` to `<MudTable>`, `<input>` to `<MudTextField>`).

### To-dos

- [ ] Add MudBlazor NuGet package and register services.
- [ ] Update index.html and app.css to use MudBlazor assets.
- [ ] Refactor MainLayout.razor and NavMenu.razor with MudBlazor components.