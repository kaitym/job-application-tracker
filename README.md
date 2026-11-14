# Job Application Tracker

**Every job application I'm tracking, in one place.**

I built this to track my own job search — companies, roles, statuses, links, and notes — instead of losing applications across spreadsheets and browser tabs. It's also how I'm learning C#/.NET, SQL, and testing for real. Right now it's a REST API backed by Postgres; next up is a coffee-shop-themed frontend I can use every day (I do most of my applying from coffee shops, so it felt right ☕).

<p>
  <img alt="CI" src="https://github.com/kaitym/job-application-tracker/actions/workflows/ci.yml/badge.svg">
  <img alt="C#" src="https://img.shields.io/badge/C%23-239120?logo=csharp&logoColor=white">
  <img alt=".NET" src="https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet&logoColor=white">
  <img alt="PostgreSQL" src="https://img.shields.io/badge/PostgreSQL-17-4169E1?logo=postgresql&logoColor=white">
  <img alt="Docker" src="https://img.shields.io/badge/Docker-2496ED?logo=docker&logoColor=white">
  <img alt="Status" src="https://img.shields.io/badge/status-in%20progress-yellow">
</p>

## What it does

| | |
|---|---|
| **Track applications** | Each job has a company, role, location, status, date applied, deadline, link, and notes. |
| **Full CRUD** | Add, list, view, update, and delete jobs through the API. |
| **Status pipeline** | Every job is in one stage: Saved → Applied → Interviewing → Offer / Rejected / Ghosted. |
| **Status history** | Every status change is saved with a timestamp, so I can see how each application moved. |
| **Order up! 🔔** | A `needsAction` flag for "they responded, my move" — the API remembers when they first replied. |
| **Gone cold** | Finds applications with no news in 30+ days, so I know what's probably a ghost. |
| **Interviews** | Schedule interviews per job and see everything coming up across all jobs. |
| **Search + filters** | Filter by status or "needs action", and search company, role, or location. |
| **Stats + levels** | Response rate, counts per stage, and XP that levels up my (future) coffee shop ☕ |
| **Survives restarts** | Everything is stored in Postgres, not in memory. |

### Endpoints

| Method | Route | What it does |
|---|---|---|
| `GET` | `/jobs` | List jobs — optional `?status=Applied`, `?needsAction=true`, `?search=irvine` |
| `POST` | `/jobs` | Add a job (returns `201` + where to find it) |
| `GET` | `/jobs/{id}` | Get one job (`404` if it doesn't exist) |
| `PUT` | `/jobs/{id}` | Update a job (status changes get added to its history) |
| `DELETE` | `/jobs/{id}` | Delete a job and its history/interviews (`204`) |
| `GET` | `/jobs/{id}/history` | Every status change for a job |
| `GET` | `/jobs/gone-cold` | Applied jobs with no news in `?days=30` |
| `GET` | `/jobs/{id}/interviews` | A job's interviews, soonest first |
| `POST` | `/jobs/{id}/interviews` | Schedule an interview |
| `DELETE` | `/jobs/{id}/interviews/{interviewId}` | Cancel an interview |
| `GET` | `/interviews/upcoming` | Interviews in the next `?days=14`, with company and role |
| `GET` | `/stats` | Counts per stage, response rate, XP, and shop level |

Company and role are required — the API returns `400` if either is blank. Example requests for all of these are in [`JobTrackerApi.http`](src/JobTrackerApi/JobTrackerApi.http).

### How leveling works

XP comes from real progress, and each job only earns each step once (no farming XP by flipping statuses back and forth):

| Step | XP |
|---|---|
| Save a job | +5 |
| Apply | +15 |
| They respond | +25 |
| Interview | +50 |
| Offer | +200 |
| Rejection | +5 (I still showed up) |

Levels start at 100 XP and go up to level 10 at 5,000 XP. Each level will unlock something new in the coffee shop.

## Tech stack

| Layer | Choice |
|---|---|
| API | C# / ASP.NET Core minimal API, .NET 9 |
| Data | PostgreSQL 17 + Entity Framework Core (migrations) |
| Local dev | Docker Compose (Postgres runs in a container) |
| Tests | xUnit + Testcontainers (tests run against a real, throwaway Postgres) |
| Frontend | React + TypeScript, coffee shop theme (planned) |
| CI | GitHub Actions (runs every test on every push) |
| Deploy | Render + Neon Postgres + Vercel, all free tiers (planned) |

## Architecture

```
┌──────────────┐    HTTP /jobs     ┌───────────────────┐
│  Browser /   │ ────────────────▶ │  JobEndpoints     │  routes, validation,
│  frontend    │ ◀──────────────── │                   │  status codes
└──────────────┘       JSON        └────────┬──────────┘
                                            ▼
                                   ┌───────────────────┐
                                   │  JobService       │  job logic
                                   └────────┬──────────┘
                                            │ EF Core (AppDbContext)
                                            ▼
                                   ┌───────────────────┐
                                   │  PostgreSQL       │
                                   │  jobs table       │
                                   │  (Docker locally) │
                                   └───────────────────┘
```

- **Each layer has one job.** `JobEndpoints` handles HTTP (routes, `400`s, `404`s), `JobService` handles the job logic and talks to the database, and `Program.cs` just wires everything up.
- **One type per file.** The `Job` record, the `JobStatus` enum, and the request shapes each have their own file.
- **The client doesn't pick the Id.** `POST` and `PUT` take request objects (`CreateJobRequest`, `UpdateJobRequest`) without an Id — Postgres generates it.
- **Statuses are an enum, not free text.** No more `"applied"` vs `"Applied "` vs `"Aplied"`. They're stored as text in the database so they're readable in SQL.
- **Tests never touch my real data.** Every test run spins up its own Postgres container, runs the migrations, and throws it away after.

### Code layout

```
JobTrackerApi/
  docker-compose.yml            local Postgres
  .github/workflows/ci.yml      runs the tests on every push
  src/JobTrackerApi/
    Program.cs                  app setup
    Jobs/
      Job.cs                    the Job record
      JobStatus.cs              pipeline stages
      JobStatusChange.cs        one entry in a job's history
      CreateJobRequest.cs       what POST takes
      UpdateJobRequest.cs       what PUT takes
      JobEndpoints.cs           /jobs routes
      JobService.cs             job logic, history, filters, gone-cold
    Interviews/
      Interview.cs              a scheduled interview
      InterviewEndpoints.cs     interview routes
      InterviewService.cs       interview logic
    Stats/
      Leveling.cs               XP and level rules
      StatsEndpoints.cs         /stats route
      StatsService.cs           counts, response rate, XP
    Data/
      AppDbContext.cs           database tables and column rules
      DbTime.cs                 UTC timestamps Postgres can store exactly
    Migrations/                 EF Core migrations
  tests/JobTrackerApi.Tests/
    ApiFactory.cs               starts the API with a test Postgres
    JobsEndpointTests.cs        job, history, filter, gone-cold tests
    InterviewEndpointsTests.cs  interview tests
    StatsEndpointsTests.cs      stats and XP tests
    LevelingTests.cs            unit tests for leveling (no database)
```

## Getting started

Requirements: [.NET 9 SDK](https://dotnet.microsoft.com/download), Docker ([OrbStack](https://orbstack.dev) or [Docker Desktop](https://www.docker.com/products/docker-desktop/)), and the EF Core CLI:

```sh
dotnet tool install --global dotnet-ef --version 9.0.23
```

Start Postgres:

```sh
docker compose up -d
```

Create the tables:

```sh
dotnet ef database update --project src/JobTrackerApi
```

Run the API:

```sh
dotnet run --project src/JobTrackerApi
```

Then open http://localhost:5181/jobs — you'll see `[]` until you add a job (try the `POST` in `JobTrackerApi.http`).

## Testing

```sh
dotnet test
```

42 tests: integration tests for every endpoint (the happy path, the "doesn't exist" case `404`, and validation `400`), plus unit tests for the leveling rules. Docker needs to be running, since each test run starts its own Postgres container with Testcontainers. GitHub Actions runs the same tests on every push — that's the CI badge at the top.

## Status

In progress. What's built: a full CRUD API split into endpoints and a service layer, status history, interviews, search and filters, gone-cold detection, stats with XP and levels, Postgres storage with EF Core migrations, Docker Compose for local dev, integration tests against real Postgres, and GitHub Actions CI. What's next: login (so only I can edit), the coffee shop frontend, and a free deploy.

Known limitation: there's no auth yet, so anyone who can reach the API can edit it. That's fine while it only runs on my laptop, and it gets fixed before anything goes live.
