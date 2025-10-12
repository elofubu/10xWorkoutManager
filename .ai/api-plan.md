# REST API Plan for 10xWorkoutManager

This document outlines the REST API for the 10xWorkoutManager application. The API is designed to support all functionalities described in the Product Requirements Document (PRD), leveraging a Supabase backend with a PostgreSQL database.

## 1. Resources

The API is structured around the following main resources, which directly correspond to the database tables:

-   **MuscleGroups**: Categories for exercises (e.g., Chest, Back).
-   **Exercises**: Predefined and user-created exercises.
-   **WorkoutPlans**: User-created workout plans.
-   **TrainingDays**: Specific days within a workout plan (e.g., Day A).
-   **Sessions**: Records of completed or ongoing training sessions.
-   **SessionExercises**: Details of each exercise performed within a session.
-   **ExerciseSets**: Details of each set performed for an exercise.

## 2. Endpoints

All endpoints require authentication. The authenticated user's ID will be automatically used for all operations, enforcing data ownership and security as defined by PostgreSQL Row-Level Security policies.

---

### **Muscle Groups**

#### `GET /muscle-groups`

-   **Description**: Retrieves a list of all available muscle groups.
-   **Query Parameters**:
    -   `page` (optional, number, default: 1): The page number for pagination.
    -   `pageSize` (optional, number, default: 20): The number of items per page.
-   **Response Payload (200 OK)**:
    ```json
    {
      "data": [
        {
          "id": 1,
          "name": "Chest"
        }
      ],
      "pagination": {
        "page": 1,
        "pageSize": 20,
        "totalCount": 1
      }
    }
    ```
-   **Error Codes**: `401 Unauthorized`

---

### **Exercises**

#### `GET /exercises`

-   **Description**: Retrieves a list of all predefined and user-created exercises.
-   **Query Parameters**:
    -   `search` (optional, string): Filter exercises by name (case-insensitive search).
    -   `muscleGroupId` (optional, number): Filter exercises by muscle group ID.
    -   `page` (optional, number, default: 1): Page number.
    -   `pageSize` (optional, number, default: 20): Items per page.
-   **Response Payload (200 OK)**:
    ```json
    {
      "data": [
        {
          "id": 101,
          "userId": "user-uuid-123",
          "muscleGroupId": 1,
          "name": "Bench Press"
        }
      ],
      "pagination": {
        "page": 1,
        "pageSize": 20,
        "totalCount": 1
      }
    }
    ```
-   **Error Codes**: `401 Unauthorized`

#### `POST /exercises`

-   **Description**: Creates a new custom exercise for the authenticated user.
-   **Request Payload**:
    ```json
    {
      "name": "Custom Bicep Curl",
      "muscleGroupId": 5
    }
    ```
-   **Response Payload (201 Created)**:
    ```json
    {
      "id": 102,
      "userId": "user-uuid-123",
      "muscleGroupId": 5,
      "name": "Custom Bicep Curl"
    }
    ```
-   **Error Codes**: `400 Bad Request`, `401 Unauthorized`, `409 Conflict` (if exercise with the same name already exists for the user).

---

### **Workout Plans**

#### `GET /workout-plans`

-   **Description**: Retrieves a list of all workout plans for the authenticated user.
-   **Query Parameters**:
    -   `page` (optional, number, default: 1): Page number.
    -   `pageSize` (optional, number, default: 20): Items per page.
-   **Response Payload (200 OK)**:
    ```json
    {
      "data": [
        {
          "id": 1,
          "name": "My Strength Plan",
          "createdAt": "2025-10-12T10:00:00Z"
        }
      ],
      "pagination": {
        "page": 1,
        "pageSize": 20,
        "totalCount": 1
      }
    }
    ```
-   **Error Codes**: `401 Unauthorized`

#### `GET /workout-plans/{id}`

-   **Description**: Retrieves the details of a specific workout plan, including its training days and their exercises.
-   **Response Payload (200 OK)**:
    ```json
    {
      "id": 1,
      "name": "My Strength Plan",
      "isLocked": false,
      "trainingDays": [
        {
          "id": 1,
          "name": "Day A",
          "order": 1,
          "exercises": [
            {
              "planDayExerciseId": 1,
              "exerciseId": 101,
              "name": "Bench Press",
              "order": 1
            }
          ]
        }
      ]
    }
    ```
-   **Error Codes**: `401 Unauthorized`, `404 Not Found`

#### `POST /workout-plans`

-   **Description**: Creates a new workout plan with its associated training days.
-   **Request Payload**:
    ```json
    {
      "name": "New Hypertrophy Plan",
      "trainingDays": [
        { "name": "Push Day", "order": 1 },
        { "name": "Pull Day", "order": 2 },
        { "name": "Leg Day", "order": 3 }
      ]
    }
    ```
-   **Response Payload (201 Created)**:
    ```json
    {
      "id": 2,
      "name": "New Hypertrophy Plan",
      "trainingDays": [
        { "id": 4, "name": "Push Day", "order": 1 },
        { "id": 5, "name": "Pull Day", "order": 2 },
        { "id": 6, "name": "Leg Day", "order": 3 }
      ]
    }
    ```
-   **Error Codes**: `400 Bad Request`, `401 Unauthorized`

#### `PUT /workout-plans/{id}`

-   **Description**: Updates a workout plan's name and the order of its training days. Cannot be used if the plan is locked by an active session.
-   **Request Payload**:
    ```json
    {
      "name": "Updated Hypertrophy Plan",
      "trainingDays": [
        { "id": 6, "order": 1 },
        { "id": 4, "order": 2 },
        { "id": 5, "order": 3 }
      ]
    }
    ```
-   **Response Payload (200 OK)**: The updated workout plan object.
-   **Error Codes**: `400 Bad Request`, `401 Unauthorized`, `403 Forbidden` (if plan is locked), `404 Not Found`.

#### `DELETE /workout-plans/{id}`

-   **Description**: Deletes a workout plan and all its associated data.
-   **Response (204 No Content)**.
-   **Error Codes**: `401 Unauthorized`, `403 Forbidden` (if plan is locked), `404 Not Found`.

---

### **Plan Day Exercises** (Managed via Workout Plans)

#### `POST /workout-plans/{planId}/training-days/{dayId}/exercises`

-   **Description**: Adds an exercise to a specific training day.
-   **Request Payload**:
    ```json
    {
      "exerciseId": 105,
      "order": 3
    }
    ```
-   **Response Payload (201 Created)**:
    ```json
    {
      "id": 50,
      "trainingDayId": 4,
      "exerciseId": 105,
      "order": 3
    }
    ```
-   **Error Codes**: `400 Bad Request`, `401 Unauthorized`, `403 Forbidden` (if plan is locked), `404 Not Found`.

#### `DELETE /workout-plans/{planId}/training-days/{dayId}/exercises/{planDayExerciseId}`

-   **Description**: Removes an exercise from a training day.
-   **Response (204 No Content)**.
-   **Error Codes**: `401 Unauthorized`, `403 Forbidden` (if plan is locked), `404 Not Found`.

---

### **Sessions**

#### `GET /sessions`

-   **Description**: Retrieves a chronological list of the user's past training sessions.
-   **Query Parameters**:
    -   `page` (optional, number, default: 1): Page number.
    -   `pageSize` (optional, number, default: 20): Items per page.
-   **Response Payload (200 OK)**:
    ```json
    {
      "data": [
        {
          "id": 1,
          "planId": 1,
          "notes": "Felt strong today.",
          "startTime": "2025-10-11T09:00:00Z",
          "endTime": "2025-10-11T10:15:00Z"
        }
      ],
      "pagination": { "page": 1, "pageSize": 20, "totalCount": 1 }
    }
    ```
-   **Error Codes**: `401 Unauthorized`.

#### `GET /sessions/{id}`

-   **Description**: Retrieves the details of a specific session, including all exercises and their sets.
-   **Response Payload (200 OK)**:
    ```json
    {
      "id": 1,
      "notes": "Felt strong today.",
      "startTime": "...",
      "endTime": "...",
      "exercises": [
        {
          "id": 1,
          "exerciseId": 101,
          "notes": "Good form.",
          "skipped": false,
          "order": 1,
          "sets": [
            { "id": 1, "weight": 100, "reps": 8, "isFailure": false, "order": 1 },
            { "id": 2, "weight": 100, "reps": 7, "isFailure": true, "order": 2 }
          ]
        }
      ]
    }
    ```
-   **Error Codes**: `401 Unauthorized`, `404 Not Found`.

#### `POST /sessions`

-   **Description**: Starts a new training session based on a training day. This creates the session and populates it with exercises from the plan.
-   **Request Payload**:
    ```json
    {
      "trainingDayId": 4
    }
    ```
-   **Response Payload (201 Created)**: The full session object, ready for the workout.
-   **Error Codes**: `400 Bad Request`, `401 Unauthorized`, `404 Not Found` (training day not found).

#### `PUT /sessions/{id}`

-   **Description**: Updates a session, typically to add notes or mark it as complete by setting the `endTime`.
-   **Request Payload**:
    ```json
    {
      "notes": "Finished the workout.",
      "endTime": "2025-10-12T11:30:00Z"
    }
    ```
-   **Response Payload (200 OK)**: The updated session object.
-   **Error Codes**: `400 Bad Request`, `401 Unauthorized`, `404 Not Found`.

#### `PUT /sessions/{sessionId}/exercises/{sessionExerciseId}`

-   **Description**: Updates a specific exercise within a session (e.g., adds notes, marks as skipped).
-   **Request Payload**:
    ```json
    {
      "notes": "Focus on form next time.",
      "skipped": false,
      "sets": [
        { "weight": 50, "reps": 12, "isFailure": false, "order": 1 },
        { "weight": 50, "reps": 10, "isFailure": true, "order": 2 }
      ]
    }
    ```
-   **Response Payload (200 OK)**: The updated session exercise object.
-   **Error Codes**: `400 Bad Request`, `401 Unauthorized`, `404 Not Found`.

#### `GET /exercises/{exerciseId}/previous-session`

-   **Description**: Retrieves the performance details (sets) of a specific exercise from the user's most recent session where it was performed.
-   **Response Payload (200 OK)**:
    ```json
    {
      "sessionDate": "2025-10-10T09:00:00Z",
      "notes": "Previous notes on this exercise.",
      "sets": [
        { "weight": 95, "reps": 8, "isFailure": false },
        { "weight": 95, "reps": 8, "isFailure": false }
      ]
    }
    ```
-   **Error Codes**: `401 Unauthorized`, `404 Not Found` (if the exercise has never been performed).

## 3. Authentication and Authorization

-   **Authentication**: The API will use JWT (JSON Web Tokens) provided by Supabase Auth. The client application is responsible for handling user login and registration via the Supabase SDK. Every request to the API must include an `Authorization` header with a valid JWT: `Authorization: Bearer <SUPABASE_JWT>`.
-   **Authorization**: Authorization is enforced at the database level using PostgreSQL Row-Level Security (RLS) policies. All tables with user-specific data are protected by policies that check if the `user_id` column matches the ID of the authenticated user (`auth.uid()`). This ensures users can only access and modify their own data.

## 4. Validation and Business Logic

-   **Validation**: Input validation is enforced at the API gateway or service layer before data reaches the database. This includes:
    -   **Presence Checks**: Fields marked as `NOT NULL` in the schema are required.
    -   **Type Checks**: Data types must match the schema (e.g., `weight` must be a number).
    -   **Value Constraints**: `CHECK` constraints (e.g., `weight >= 0`, `reps >= 0`) are enforced.
    -   **Uniqueness**: `UNIQUE` constraints (e.g., a user cannot have two custom exercises with the same name) are enforced.
-   **Business Logic**:
    -   **Workout Plan Locking**: Before any `PUT` or `DELETE` operation on `/workout-plans` or its sub-resources, the API checks for an active session (`sessions` table where `end_time` is `NULL`) linked to that `plan_id`. If one exists, a `403 Forbidden` error is returned.
    -   **Starting a Session**: The `POST /sessions` endpoint reads the exercises from the specified `training_day_id` and uses them to create corresponding records in the `session_exercises` table for the new session.
    -   **Fetching Previous Results**: The `GET /exercises/{exerciseId}/previous-session` endpoint queries the `exercise_sets` and `session_exercises` tables, filtered by the current user and the specified `exercise_id`, ordering by date to find the most recent performance data.
