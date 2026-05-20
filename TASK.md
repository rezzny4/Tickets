# Tickets API — Take-home Task

Thanks for taking the time! This is a small extension to an existing
event-sourced ticketing API built on **WolverineFx.Http** and **MartenDB**.

**Time budget:** ~1–2 hours. Don't go over 3 — if you get stuck, send what
you have along with notes on where you got blocked. We care about how you
think, not whether you finish 100%.

---

## What's already here

A minimal support-ticket service. A `Ticket` is an aggregate built from
events:

| Command          | Event             | Allowed when status is |
| ---------------- | ----------------- | ---------------------- |
| `OpenTicket`     | `TicketOpened`    | (creates a new ticket) |
| `AssignTicket`   | `TicketAssigned`  | `Open` or `Assigned`   |
| `ResolveTicket`  | `TicketResolved`  | `Assigned`             |

There is also a `TicketSummary` read-model projection used by the
`GET /tickets` endpoint.

Run it:

```bash
docker compose up -d        # starts Postgres
dotnet run --project src/Tickets.Api
dotnet test
```

Then poke at `http://localhost:3000/swagger` — the existing flows should
work end-to-end.

---

## Your task

#### Add the ability to **close** a resolved ticket.

### Requirements

1. New command `CloseTicket(Guid TicketId, string ClosedBy)` exposed at
   `POST /tickets/{id}/close`.
2. New event `TicketClosed(string ClosedBy, DateTimeOffset ClosedAt)`.
3. A ticket can only be closed when its current status is `Resolved`.
   Closing in any other status must return `400 Bad Request` with a clear
   error message.
4. Once closed, no further commands are allowed on the ticket — assigning
   or resolving a closed ticket must also fail.
5. The `TicketSummary` projection must reflect the new `Closed` status so
   `GET /tickets?status=Closed` works.
6. Add **at least one** test covering the happy path and **at least one**
   test for an invalid transition (e.g. closing an `Open` ticket).

### Out of scope (don't do these)

- Authentication / authorization
- Reopening tickets
- Migrations for already-persisted tickets
- UI

---

## What we're looking for

- You read the existing code before adding to it (match the style).
- Domain rules live in the aggregate, not the endpoint.
- Tests actually exercise the rule, not just "200 OK".
- Honest commit messages / notes — "I wasn't sure about X, here's what I
  tried" is a great answer.

If something in the scaffold looks wrong or confusing, say so. We'd
rather hear "this seems off" than have you silently work around it.

## How to apply

1. **Clone this repository** locally. Do **not** fork it on GitHub.
2. **Create a new repository** under your own GitHub account (public, or
   private with access granted to us) and push the cloned code there as
   your `main` branch.
3. **Complete the task** on a feature branch in your repo (e.g.
   `feature/close-ticket`) and commit your work.
4. **Open a Pull Request inside your own repository** — from your feature
   branch into your repo's `main` branch.
5. **Send us the link to your repository** (and the PR within it).

> Please do not open a PR against the upstream/original repo — keep all
> work and the PR contained inside your own repository.
