# Aihrly — Junior Backend Developer Take-Home Assessment

A minimal Applicant Tracking System (ATS) pipeline API built with ASP.NET Core 9, PostgreSQL, and EF Core.

---

## Running Locally

### Option 1 — Docker (recommended, zero setup)

Requires Docker Desktop running.

```bash
# Start everything — Postgres, pgAdmin, and the API
docker-compose up --build
```

| Service  | URL                          |
|----------|------------------------------|
| API      | http://localhost:8080        |
| Scalar UI| http://localhost:8080/scalar/v1 |
| pgAdmin  | http://localhost:5050        |

pgAdmin login: `admin@aihrly.com` / `admin`
DB connection in pgAdmin: host=`postgres`, port=`5432`, user=`aihrly_user`, pass=`aihrly_pass`

Migrations run automatically on startup.

---

### Option 2 — Local .NET (requires Postgres running separately)

**Prerequisites**
- .NET 9 SDK
- PostgreSQL 14+
- EF Core CLI: `dotnet tool install --global dotnet-ef`

```bash
# 1. Start Postgres only
docker-compose up postgres -d

# 2. Restore packages
dotnet restore

# 3. Apply migrations (creates schema + seeds team members)
dotnet ef database update --project src/Aihrly.Api

# 4. Run the API
dotnet run --project src/Aihrly.Api
```

API: http://localhost:5000
Scalar UI: http://localhost:5000/scalar/v1

---

## Running the Tests

Tests require Docker running — the integration tests spin up a real Postgres container automatically via Testcontainers.

```bash
# Run all tests
dotnet test

# Unit tests only (no Docker needed — pure logic, no DB)
dotnet test --filter "Aihrly.Api.Tests.Unit"

# Integration tests only
dotnet test --filter "Aihrly.Api.Tests.Integration"
```

---

## Seeded Team Members

Three team members are seeded on first migration. Use their IDs in the `X-Team-Member-Id` header for any endpoint that requires it.

| Name           | ID                                   | Role           |
|----------------|--------------------------------------|----------------|
| Alice Mensah   | a1b2c3d4-e5f6-7890-abcd-ef1234567890 | Recruiter      |
| Kwame Boateng  | b2c3d4e5-f6a7-8901-bcde-f12345678901 | HiringManager  |
| Sara Osei      | c3d4e5f6-a7b8-9012-cdef-123456789012 | Recruiter      |

**Example request with the header:**
```bash
curl -X PATCH http://localhost:5000/api/applications/{id}/stage \
  -H "Content-Type: application/json" \
  -H "X-Team-Member-Id: a1b2c3d4-e5f6-7890-abcd-ef1234567890" \
  -d '{"stage": "Screening"}'
```

---

## Pipeline Stages

```
Applied → Screening → Interview → Offer → Hired (terminal)
                                        ↘ Rejected (terminal, from any stage)
```

Any jump that skips a stage (e.g. Applied → Hired) returns `400` with a clear error message.

---

## Part 2 — Background Job (Option A)

When an application moves to `hired` or `rejected`, a notification is dispatched asynchronously:

- The `PATCH /stage` endpoint writes to the DB and returns `204` immediately
- A `NotificationWorker` (`BackgroundService`) reads from an in-memory `Channel<T>` queue
- The worker inserts a row into the `notifications` table and writes a log line
- The queue is backed by `System.Threading.Channels` — no external dependencies (no Hangfire, no Redis)

This keeps the HTTP response fast regardless of how long the notification processing takes. In production this channel would be replaced with a durable queue (e.g. RabbitMQ, Azure Service Bus) so notifications survive restarts.

---

## Assumptions Made

- **No authentication** — identity is established via the `X-Team-Member-Id` header as specified. The header is validated against real team member records in the DB; unknown IDs are rejected with `400`.
- **Email case-insensitivity** — candidate emails are normalised to lowercase on insert so `Alice@test.com` and `alice@test.com` are treated as the same applicant.
- **Migrations on startup** — `MigrateAsync()` runs at app startup. This is appropriate for a development/assessment environment. In production, migrations would run as a separate pre-deploy step.
- **Score overwrite** — re-submitting a score for the same dimension overwrites the previous value and updates `scored_by` and `scored_at`. The previous value is not retained (see ANSWERS.md Q2b for how to add history).
- **Notification delivery is simulated** — the worker logs a line and writes to the `notifications` table. No actual email is sent.
- **Single organisation** — multi-tenancy (Option C) was not chosen. All data belongs to one implicit organisation.

---

## What I'd Improve With More Time

- **Durable notification queue** — replace `System.Threading.Channels` with a persistent message broker so notifications aren't lost on app restart.
- **Pagination on notes and stage history** — currently the profile endpoint returns all notes and history. For active jobs with many candidates this could get large.
- **Cursor-based pagination** — the current offset/page approach can miss or duplicate rows if data changes between pages. Cursor pagination is more correct for live data.
- **Structured logging** — add correlation IDs to every request so logs for a single operation can be traced end-to-end.
- **OpenAPI annotations** — add `[ProducesResponseType]` attributes to controllers so the Scalar UI shows accurate response shapes.
- **Score history** — as discussed in ANSWERS.md, the current schema overwrites scores. Appending with a `superseded_at` column would support audit history.
- **Docker health in tests** — the `Task.Delay` in notification tests works but a polling assertion (retry for up to N seconds) would be more robust and faster.
