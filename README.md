# Tickets API

A small **event-sourced** support-ticket service used as a take-home
exercise for C# candidates. The service exposes a handful of HTTP
endpoints to open, assign, resolve, and list support tickets — but
internally every state change is stored as an immutable event, and the
list view is built from a projection over those events.

If you're a candidate, **start with [`TASK.md`](./TASK.md)** — it
describes the small extension you'll be implementing.

---

## Tech stack

| Piece                  | What it is / why it's here                                                |
| ---------------------- | ------------------------------------------------------------------------- |
| **.NET 10 / C#**       | Target framework for the API and tests.                                   |
| **ASP.NET Core**       | HTTP host. Swagger UI is enabled in development.                          |
| **MartenDB**           | Event store + document database on top of PostgreSQL. Persists every event for a ticket as a stream and stores the read-model (`TicketSummary`) as a JSON document. |
| **WolverineFx**        | In-process messaging / runtime that composes handlers and integrates transactionally with Marten. |
| **WolverineFx.Http**   | Endpoint framework — controllers are static methods marked with `[WolverinePost]` / `[WolverineGet]`. |
| **PostgreSQL 17**      | The database under Marten. Started locally via `docker compose`.          |
| **xUnit + Shouldly**   | Test framework + assertion library used in `tests/Tickets.Tests`.         |

> Don't worry if you've never used Wolverine or Marten before — the
> existing endpoints show every pattern you'll need.

### Why event sourcing?

Instead of storing the *current* state of a ticket (`Status = Resolved`),
we store the **sequence of events** that led to it
(`TicketOpened` → `TicketAssigned` → `TicketResolved`). The current
state is rebuilt by replaying those events through the `Ticket`
aggregate. A separate **projection** (`TicketSummary`) listens to the
same events and maintains a flat, queryable read model for list/detail
endpoints.

The reason to know this for the task: **business rules live in the
aggregate / decider, not in the endpoint**. The endpoint loads the
aggregate, asks the decider what event (if any) should be appended, and
saves. That's the pattern you'll be extending.

---

## Project layout

```
src/Tickets.Api/
  Domain/
    Ticket.cs              aggregate — current state, rebuilt from events
    Events.cs              TicketOpened / TicketAssigned / TicketResolved
    Commands.cs            request payloads (OpenTicket, AssignTicket, ...)
    TicketDecider.cs       pure functions: (state, command) -> event, enforce rules
    TicketStatus.cs        Open | Assigned | Resolved
  Endpoints/
    TicketEndpoints.cs     WolverineFx.Http endpoints (POST/GET /tickets/...)
  Projections/
    TicketSummary.cs       single-stream projection -> list/detail read model
  Program.cs               composition root: Marten + Wolverine + Swagger
  appsettings.json         connection string

tests/Tickets.Tests/
  TicketDeciderTests.cs    examples of testing the rules without a database

docker-compose.yml         Postgres 17 for local dev
TASK.md                    the take-home task description
```

---

## Existing behavior

| Method | Route                          | Effect                                                |
| ------ | ------------------------------ | ----------------------------------------------------- |
| POST   | `/tickets`                     | Opens a new ticket (emits `TicketOpened`).            |
| POST   | `/tickets/{id}/assign`         | Assigns a ticket (emits `TicketAssigned`).            |
| POST   | `/tickets/{id}/resolve`        | Resolves an Assigned ticket (emits `TicketResolved`). |
| GET    | `/tickets/{id}`                | Reads the `TicketSummary` projection.                 |
| GET    | `/tickets?status=Open`         | Lists summaries, optionally filtered by status.       |

Allowed transitions:

```
                ┌─ AssignTicket ─┐
                ↓                 │
   (none) → Open ─→ Assigned ──→ Resolved
                  AssignTicket   ResolveTicket
```

Invalid transitions (e.g. resolving an `Open` ticket) return
`400 Bad Request` with an error message — see `TicketDecider` for the
rules.

---

## Prerequisites

- .NET 10 SDK
- Docker (only used to run Postgres locally)

## Run it

```bash
docker compose up -d                     # starts Postgres on :5432
dotnet run --project src/Tickets.Api     # starts the API on :3000
```

Then open Swagger UI: <http://localhost:3000/swagger>

A quick smoke test from the command line:

```bash
# open a ticket
curl -s -X POST http://localhost:3000/tickets \
  -H 'content-type: application/json' \
  -d '{"title":"Login broken","description":"500 on /login"}'
# -> { "id": "..." }

# assign it
curl -s -X POST http://localhost:3000/tickets/<id>/assign \
  -H 'content-type: application/json' \
  -d '{"assignee":"alice"}'

# list open & assigned tickets
curl -s 'http://localhost:3000/tickets'
```

## Test it

```bash
dotnet test
```

Tests run against the in-memory decider — **no database required**.
Look at `TicketDeciderTests.cs` for the style we expect.

---

## Tips for the task

- Read `TicketDecider.cs` first. The patterns for "command in, event
  out, throw on invalid transition" are all there.
- A new event needs four touchpoints: the event record, an `Apply`
  method on `Ticket`, a rule in `TicketDecider`, and a branch in
  `TicketSummaryProjection`. Don't forget the projection — it's the
  most commonly missed step.
- Don't fight the framework. If something feels like it should be
  one line and you're writing twenty, you're probably off the happy
  path — re-read an existing endpoint.
- It's fine to leave a `TODO` or a note if you run out of time. We'd
  rather see honest, working code than a half-broken big swing.
