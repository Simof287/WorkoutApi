# Workout API

A REST API for tracking workouts, exercises, and sets. The project is built with ASP.NET Core and .NET 10, uses ASP.NET Core Identity with JWT bearer authentication, and stores data in SQLite through Entity Framework Core.

> **Project status:** backend API and integration tests are implemented. A web frontend and hosted demo are not included yet.

## Features

- Register an account and sign in to receive a JWT access token.
- Browse and filter the exercise catalog with pagination.
- Create, list, update, and delete workouts belonging to the authenticated user.
- Store ordered exercises and sets with repetitions and weight in each workout.
- Restrict exercise catalog writes to users with the `Admin` role.
- Return structured Problem Details responses for API errors.
- Exercise the API through xUnit integration tests using an in-memory SQLite database.

## Tech stack

- .NET 10 / ASP.NET Core Web API
- ASP.NET Core Identity and JWT bearer authentication
- Entity Framework Core with SQLite and migrations
- OpenAPI and Swagger UI
- xUnit and `WebApplicationFactory` integration tests

## Run locally

### Requirements

- .NET 10 SDK
- The `dotnet-ef` tool for applying database migrations

From the repository root, configure a local JWT signing key with .NET User Secrets. Use a private random value of at least 32 UTF-8 bytes; do not commit it to source control.

```bash
cd WorkoutApi
dotnet user-secrets set "Jwt:SigningKey" "replace-with-a-private-random-key-of-at-least-32-bytes"
```

Install the EF Core command-line tool if it is not already installed, then create the local SQLite database from the committed migrations:

```bash
dotnet tool install --global dotnet-ef
dotnet ef database update
```

Start the API using the included HTTP development profile:

```bash
dotnet run --launch-profile http
```

The API listens at `http://localhost:5295`. In Development, Swagger UI is available at `http://localhost:5295/swagger` and the OpenAPI document at `http://localhost:5295/openapi/v1.json`.

The SQLite database is created as `WorkoutApi/workout.db`. It is local application data and should not be committed.

## Run the tests

From the repository root:

```bash
dotnet test WorkoutApi/WorkoutApi.Tests/WorkoutApi.Tests.csproj
```

The integration tests use a separate in-memory SQLite database; they do not need the local `workout.db` file.

## First-time setup and admin access

The exercise catalog starts empty. Exercise listing is public, but creating, updating, and deleting exercises requires the `Admin` role.

To promote an account for local development:

1. Start the API and register an account using `POST /api/auth/register`.
2. Stop the API.
3. Set `AdminSeed:Email` to the registered account's email in User Secrets:

   ```bash
   dotnet user-secrets set "AdminSeed:Email" "you@example.com"
   ```

4. Restart the API. Startup assigns the `Admin` role to the existing account with that email.

The seed process does not create an admin account; the account must already exist. Do not use a public or production account for this local setup.

## API overview

All routes are prefixed with `/api`. JSON uses camelCase fields.

| Method | Route | Access | Description |
|---|---|---|---|
| `POST` | `/api/auth/register` | Public | Create an account |
| `POST` | `/api/auth/login` | Public | Sign in and receive a JWT |
| `GET` | `/api/exercises` | Public | List exercises; supports `muscleGroup`, `page`, and `pageSize` |
| `GET` | `/api/exercises/{id}` | Public | Get one exercise |
| `POST` | `/api/exercises` | Admin | Create an exercise |
| `PUT` | `/api/exercises/{id}` | Admin | Update an exercise |
| `DELETE` | `/api/exercises/{id}` | Admin | Delete an exercise |
| `POST` | `/api/workouts` | Authenticated | Create a workout |
| `GET` | `/api/workouts` | Authenticated | List the current user's workouts |
| `GET` | `/api/workouts/{id}` | Authenticated | Get one of the current user's workouts |
| `PUT` | `/api/workouts/{id}` | Authenticated | Update workout date and notes |
| `DELETE` | `/api/workouts/{id}` | Authenticated | Delete a workout |

Pagination defaults to `page=1&pageSize=10`; `page` must be at least 1 and `pageSize` must be between 1 and 100.

### Authentication example

Register, then sign in:

```bash
curl -X POST http://localhost:5295/api/auth/register \
  -H 'Content-Type: application/json' \
  -d '{"userName":"demo-user","email":"demo@example.com","password":"StrongPassword123!"}'

curl -X POST http://localhost:5295/api/auth/login \
  -H 'Content-Type: application/json' \
  -d '{"email":"demo@example.com","password":"StrongPassword123!"}'
```

The login response contains `accessToken`, `tokenType`, and `expiresAt`. Send the token on workout requests:

```bash
curl http://localhost:5295/api/workouts \
  -H 'Authorization: Bearer <accessToken>'
```

### Create a workout

Replace `exerciseId` with an existing exercise ID. An exercise must have at least one set; exercise order values must be unique.

```bash
curl -X POST http://localhost:5295/api/workouts \
  -H 'Authorization: Bearer <accessToken>' \
  -H 'Content-Type: application/json' \
  -d '{
    "date":"2026-11-01T09:30:00Z",
    "notes":"Upper body session",
    "exercises":[
      {
        "exerciseId":1,
        "order":1,
        "sets":[
          {"repetitions":10,"weight":40.0},
          {"repetitions":8,"weight":45.0}
        ]
      }
    ]
  }'
```

If `date` is omitted when creating a workout, the API uses the current UTC time. Workout updates accept optional `date` and `notes`; omitted or `null` values leave the stored values unchanged. An empty string can be used to clear notes.

## Error responses

Errors use Problem Details with `application/problem+json`. Responses include a stable `type`, a short `title`, the HTTP `status`, and a `traceId` for log correlation. Input validation responses include an `errors` object keyed by field. Clients should use these fields rather than parse free-form message text.

Example validation response:

```json
{
  "type": "urn:workout-api:problems:validation-error",
  "title": "Validation failed",
  "status": 400,
  "errors": {
    "page": ["The field page must be between 1 and 2147483647."]
  },
  "traceId": "00-example"
}
```

## Configuration

| Setting | Purpose | Local default |
|---|---|---|
| `ConnectionStrings:WorkoutDatabase` | SQLite connection string | `Data Source=workout.db` |
| `Jwt:Issuer` | JWT issuer | `WorkoutApi` |
| `Jwt:Audience` | JWT audience | `WorkoutApi` |
| `Jwt:SigningKey` | Private signing key (at least 32 UTF-8 bytes) | Must be supplied locally |
| `Jwt:TokenLifetimeMinutes` | Token lifetime | `120` minutes |
| `AdminSeed:Email` | Existing account to grant the `Admin` role at startup | Not set |

For environment variables, use the ASP.NET Core double-underscore format, such as `Jwt__SigningKey` and `AdminSeed__Email`. Keep secrets outside the repository.

## Project layout

```text
WorkoutApi/                 API, data layer, migrations, and integration tests
docs/ERROR_MESSAGES_PLAN.md Error response standardization plan
FULLSTACK_PLAN.md           Full-stack product and delivery roadmap
```

## Roadmap

- Build a responsive client for registration, exercise browsing, and workout tracking.
- Add a repeatable sample exercise catalog and an accessible demo path.
- Choose and document the browser session strategy before implementing frontend authentication.
- Add CI, container/deployment configuration, and production database guidance.

## License

No license has been added yet. Contact the repository owner before reusing this project.
