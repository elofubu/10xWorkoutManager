# UI Architecture for 10xWorkoutManager

## 1. UI Structure Overview

This document outlines the user interface architecture for the 10xWorkoutManager MVP. The application will be a Blazor WebAssembly Single Page Application (SPA) utilizing the MudBlazor component library for a consistent and responsive user experience.

The architecture is designed around a set of distinct views, each corresponding to a major user task (e.g., managing plans, performing a workout). A mobile-first design philosophy ensures core functionality is optimized for touchscreens, while providing a rich experience on desktop. State management will be handled on a per-view basis, with global user state (authentication) managed by the Supabase SDK. Navigation is centralized through a main layout containing a persistent navigation menu, ensuring predictable user flow.

## 2. View List

### 2.1. Authentication Views

#### **Login View**
- **View Path**: `/login`
- **Main Purpose**: To allow existing users to sign in.
- **Key Information**: Email and password fields.
- **Key View Components**: `MudForm`, `MudTextField` (for email/password), `MudButton` (for submission), `MudLink` (to registration and password reset).
- **UX, Accessibility, and Security**:
  - **UX**: Clear error messages for invalid credentials. Button enters a loading state on submission.
  - **Accessibility**: Proper labels for form fields, keyboard navigable.
  - **Security**: All communication via HTTPS. No storing of passwords client-side.

#### **Registration View**
- **View Path**: `/register`
- **Main Purpose**: To allow new users to create an account. (PRD: US-001)
- **Key Information**: Email, password, and confirm password fields.
- **Key View Components**: `MudForm`, `MudTextField`, `MudButton`, `MudLink` (to login).
- **UX, Accessibility, and Security**:
  - **UX**: Real-time validation for email format and password matching.
  - **Accessibility**: Accessible labels and validation messages.
  - **Security**: Password strength indicators are recommended for a future version.

#### **Password Reset View**
- **View Path**: `/reset-password`
- **Main Purpose**: To enable users who have forgotten their password to reset it. (PRD: US-003)
- **Key Information**: Email address field to send the reset link. A subsequent view for entering a new password.
- **Key View Components**: `MudForm`, `MudTextField`, `MudButton`.
- **UX, Accessibility, and Security**:
  - **UX**: Clear confirmation message after the email has been sent.
  - **Security**: The reset link sent via email must be single-use and time-limited.

### 2.2. Main Application Views

#### **Dashboard / Workout Plans List**
- **View Path**: `/`
- **Main Purpose**: To display all of the user's workout plans and serve as the main entry point. (PRD: US-007)
- **Key Information**: A list of workout plan names.
- **Key View Components**: `MudCard` for each plan, `MudButton` (FAB - Floating Action Button) to create a new plan, `EmptyState` component with a CTA for new users. (PRD: US-005)
- **UX, Accessibility, and Security**:
  - **UX**: Clicking a plan card navigates to its detail view. The list should be easily scannable.
  - **Accessibility**: Plan names should be headings or list items for screen readers.
  - **API Interaction**: `GET /workout-plans`.

#### **Workout Plan Detail View**
- **View Path**: `/plans/{id}`
- **Main Purpose**: To display the details of a single workout plan, including its training days and exercises, and to allow for editing and initiating a session. (PRD: US-008)
- **Key Information**: Plan name, list of training days, and under each day, a list of its exercises.
- **Key View Components**: `MudList` or `MudExpansionPanels` for training days, drag-and-drop lists for reordering days and exercises (in edit mode), `MudButton` to toggle edit mode, `MudButton` per day to "Start Workout".
- **UX, Accessibility, and Security**:
  - **UX**: A clear distinction between view and edit modes. If the plan is locked (`isLocked: true`), all editing controls are disabled with an explanatory `MudAlert`. (PRD: US-009)
  - **Accessibility**: Drag-and-drop should have keyboard alternatives.
  - **API Interaction**: `GET /workout-plans/{id}`, `PUT /workout-plans/{id}`, `POST /.../exercises`, `DELETE /.../exercises`.

#### **Workout Session View**
- **View Path**: `/session/{id}`
- **Main Purpose**: To guide the user through an active workout, allowing them to record their performance for each exercise. (PRD: US-016)
- **Key Information**: Current exercise name, previous performance data, input fields for sets (weight, reps), notes.
- **Key View Components**: `MudStepper` or `MudTabs` for exercise navigation, `MudCard` to display current exercise details, table-like structure for logging sets (`MudNumericField`, `MudCheckBox` for "to failure"), `MudTextField` for notes, `MudButton` for "Save and Continue", "Skip Exercise", and "Finish Workout".
- **UX, Accessibility, and Security**:
  - **UX**: Pre-fetch previous performance data for all exercises at the start to ensure smooth transitions. A loading indicator will be shown. (PRD: US-017) Buttons provide clear async feedback.
  - **Accessibility**: Inputs must be large and easy to use on mobile.
  - **API Interaction**: `POST /sessions` (to start), `GET /exercises/{exerciseId}/previous-session` (for each exercise), `PUT /sessions/{sessionId}/exercises/{sessionExerciseId}` (on "Save and Continue"), `PUT /sessions/{id}` (on "Finish Workout").

#### **Workout History View**
- **View Path**: `/history`
- **Main Purpose**: To provide a chronological list of all completed workout sessions. (PRD: US-023)
- **Key Information**: List of sessions with date, plan name, and day name.
- **Key View Components**: `MudList` or `MudTable`, `MudPagination` (on desktop) or infinite scroll (on mobile). `EmptyState` component if no history exists.
- **UX, Accessibility, and Security**:
  - **UX**: Easy navigation to a specific session's summary by clicking an item.
  - **API Interaction**: `GET /sessions`.

#### **Workout Session Summary View**
- **View Path**: `/history/{id}`
- **Main Purpose**: To show a detailed summary of a single completed workout. (PRD: US-024)
- **Key Information**: Session date, duration, notes, and a full breakdown of each exercise with its sets, reps, weight, and notes.
- **Key View Components**: `MudCard` for overall summary, `MudDescriptionList` or similar to display exercise details.
- **UX, Accessibility, and Security**:
  - **UX**: A read-only, clear, and concise presentation of performance data.
  - **API Interaction**: `GET /sessions/{id}`.

#### **Settings View**
- **View Path**: `/settings`
- **Main Purpose**: To provide access to account management features.
- **Key Information**: Account-related actions.
- **Key View Components**: `MudButton` for "Delete Account" which triggers a `ConfirmationDialog`.
- **UX, Accessibility, and Security**:
  - **Security**: The "Delete Account" action must be heavily guarded with a confirmation dialog requiring the user to type a confirmation phrase to prevent accidental deletion. (PRD: US-004)

## 3. User Journey Map

1.  **Onboarding**: `Register` -> Auto-login -> `Dashboard` (with empty state) -> Click CTA -> `Plan Creation Wizard`.
2.  **Plan Management**: From `Dashboard`, click a plan -> `Plan Detail View`.
    -   In `Plan Detail`, click "Edit". Add an exercise via `ExercisePickerDialog`. Create a new exercise via `CreateExerciseDialog`. Reorder items via drag-and-drop. Save changes.
3.  **Workout Execution**: From `Plan Detail`, click "Start Workout" on a training day -> `Workout Session View`.
    -   User cycles through exercises, logging data.
    -   On the final exercise, user clicks "Finish Workout".
    -   User is redirected to the `Workout Session Summary View`.
4.  **History Review**: From main navigation -> `History View`.
    -   User browses the list of past sessions.
    -   User clicks a session to see its details in the `Workout Session Summary View`.

## 4. Layout and Navigation Structure

-   **Main Layout**: A single, persistent layout will be used for all authenticated views.
-   **App Bar**: The top app bar will contain the current view's title and a menu icon to toggle the navigation drawer on mobile.
-   **Navigation Drawer (`NavMenu`)**: A slide-out menu will be the primary navigation method. It will contain links to:
    -   Dashboard (Plans)
    -   History
    -   Settings
    -   A "Logout" button.
-   **Routing**: Blazor's file-based routing will be used. Route guards (`[Authorize]` attribute) will protect all authenticated routes, redirecting unauthenticated users to the `/login` page.

## 5. Key Components

These are reusable components to ensure UI consistency and reduce code duplication.

-   **`ConfirmationDialog`**: A standardized `MudDialog` used for all destructive actions (e.g., delete plan, delete account). It presents a clear warning and requires explicit user confirmation.
-   **`ExercisePickerDialog`**: A `MudDialog` for adding exercises to a training day. It will feature:
    -   A search input (`GET /exercises?search=...`).
    -   A filter for muscle groups (`GET /exercises?muscleGroupId=...`).
    -   A button to open the `CreateExerciseDialog`.
-   **`CreateExerciseDialog`**: A simple form dialog for creating a new custom exercise (`POST /exercises`).
-   **`EmptyState`**: A component displayed when a list is empty (e.g., no workout plans). It includes an icon, a message, and a primary CTA button to guide the user's next action.
-   **`LoadingIndicator`**: A component providing visual feedback for asynchronous operations. It will be used in two forms: a button's internal loading state and a global overlay for page-level data fetching.