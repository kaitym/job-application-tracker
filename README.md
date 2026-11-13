# Job Application Tracker

**Every job application I'm tracking, in one place.**

I built this to track my own job search — companies, roles, statuses, links, and notes — instead of losing applications across spreadsheets and browser tabs. Right now it's a small API; the plan is a full app with a database and a frontend I can use every day.

<p>
  <img alt="C#" src="https://img.shields.io/badge/C%23-239120?logo=csharp&logoColor=white">
  <img alt=".NET" src="https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet&logoColor=white">
  <img alt="Status" src="https://img.shields.io/badge/status-prototype-lightgrey">
</p>

## What it does

| | |
|---|---|
| **Track applications** | Each job has a company, role, location, status, link, and notes. |
| **List jobs** | `GET /jobs` returns everything being tracked. |
| **Add a job** | Coming next — a `POST /jobs` endpoint so jobs can be added without editing code. |

## Tech stack

| Layer | Choice |
|---|---|
| API | C# / ASP.NET Core minimal API, .NET 9 |
| Data | In-memory list for now; Postgres next |
| Frontend | React + TypeScript (planned) |
| Tests | xUnit (planned) |
| CI | GitHub Actions (planned) |
| Deploy | AWS (planned) |

## Architecture

```
┌──────────────┐   HTTP GET /jobs   ┌───────────────────┐
│  Browser /   │ ─────────────────▶ │  ASP.NET Core API │
│  frontend    │                    │  Program.cs       │
└──────────────┘                    └────────┬──────────┘
                                             │ List<Job>
                                             ▼
                                    ┌───────────────────┐
                                    │  Job.cs (record)  │
                                    │  Company, Role,   │
                                    │  Location, Status,│
                                    │  Link, Notes      │
                                    └───────────────────┘
```

- **One type per file.** The Job shape lives in `Job.cs`; `Program.cs` only wires up the app and its routes.
- **Data lives outside the code eventually.** Right now jobs sit in a list in memory; the next step is Postgres, so jobs survive restarts.

### Code layout

```
JobTrackerApi/
  Program.cs      app setup and routes
  Job.cs          the Job record
```

## Getting started

Requirements: [.NET 9 SDK](https://dotnet.microsoft.com/download).

```sh
dotnet run
```

Then open http://localhost:5181/jobs — you'll see the list of tracked jobs (empty for now, `[]`).

## Testing

No tests yet. xUnit tests are next, starting with the Job model and the `/jobs` endpoint. GitHub Actions CI is planned.

## Status

Working prototype. What's built: the API starts, and `GET /jobs` returns the tracked jobs. What's next: `POST /jobs` to add a job, Postgres for storage, a React frontend with a form, tests, and an AWS deploy.

Known limitation: jobs are stored in memory, so they disappear when the server restarts. That's what the Postgres step fixes.
