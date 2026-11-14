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
| **Track applications** | Each job has a company, role, location, status, date applied, link, and notes. |
| **Full CRUD** | Add, list, view, update, and delete jobs through the API. |
| **Status pipeline** | Every job is in one stage: Saved → Applied → Interviewing → Offer / Rejected / Ghosted. |
| **Survives restarts** | Jobs are stored in Postgres, not in memory. |

### Endpoints

| Method | Route | What it does |
|---|---|---|
| `GET` | `/jobs` | List every job |
| `POST` | `/jobs` | Add a job (returns `201` + where to find it) |
| `GET` | `/jobs/{id}` | Get one job (`404` if it doesn't exist) |
| `PUT` | `/jobs/{id}` | Update a job |
| `DELETE` | `/jobs/{id}` | Delete a job (`204`) |

Company and role are required — the API returns `400` if either is blank. Example requests for all of these are in [`JobTrackerApi.http`](src/JobTrackerApi/JobTrackerApi.http).

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
  src/JobTrackerApi/
    Program.cs                  app setup
    Jobs/
      Job.cs                    the Job record
      JobStatus.cs              pipeline stages
      CreateJobRequest.cs       what POST takes
      UpdateJobRequest.cs       what PUT takes
      JobEndpoints.cs           /jobs routes
      JobService.cs             job logic and database access
    Data/
      AppDbContext.cs           database tables and column rules
    Migrations/                 EF Core migrations
  tests/JobTrackerApi.Tests/
    ApiFactory.cs               starts the API with a test Postgres
    JobsEndpointTests.cs        endpoint tests
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

11 tests covering every endpoint — the happy path, the "doesn't exist" case (`404`), and validation (`400`). Docker needs to be running, since each test run starts its own Postgres container with Testcontainers. GitHub Actions runs the same tests on every push — that's the CI badge at the top.

## Status

In progress. What's built: a full CRUD API split into endpoints and a service layer, Postgres storage with EF Core migrations, Docker Compose for local dev, integration tests against real Postgres, and GitHub Actions CI. What's next: filtering and stats, login (so only I can edit), the coffee shop frontend, and a free deploy.

Known limitation: there's no auth yet, so anyone who can reach the API can edit it. That's fine while it only runs on my laptop, and it gets fixed before anything goes live.
