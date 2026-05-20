using Marten;
using Microsoft.AspNetCore.Mvc;
using Tickets.Api.Domain;
using Tickets.Api.Projections;
using Wolverine.Http;

namespace Tickets.Api.Endpoints;

public static class TicketEndpoints
{
    [WolverinePost("/tickets")]
    public static async Task<IResult> Open(
        OpenTicket cmd,
        IDocumentSession session,
        TimeProvider clock,
        CancellationToken ct)
    {
        var opened = TicketDecider.Open(cmd, clock.GetUtcNow());
        var stream = session.Events.StartStream<Ticket>(opened);
        await session.SaveChangesAsync(ct);
        return Results.Created($"/tickets/{stream.Id}", new { id = stream.Id });
    }

    [WolverinePost("/tickets/{id:guid}/assign")]
    public static async Task<IResult> Assign(
        Guid id,
        [FromBody] AssignTicketBody body,
        IDocumentSession session,
        TimeProvider clock,
        CancellationToken ct)
    {
        var stream = await session.Events.FetchForWriting<Ticket>(id, ct);
        if (stream.Aggregate is null) return Results.NotFound();

        var evt = TicketDecider.Assign(
            stream.Aggregate,
            new AssignTicket(id, body.Assignee),
            clock.GetUtcNow());
        stream.AppendOne(evt);
        await session.SaveChangesAsync(ct);
        return Results.NoContent();
    }

    [WolverinePost("/tickets/{id:guid}/resolve")]
    public static async Task<IResult> Resolve(
        Guid id,
        [FromBody] ResolveTicketBody body,
        IDocumentSession session,
        TimeProvider clock,
        CancellationToken ct)
    {
        var stream = await session.Events.FetchForWriting<Ticket>(id, ct);
        if (stream.Aggregate is null) return Results.NotFound();

        var evt = TicketDecider.Resolve(
            stream.Aggregate,
            new ResolveTicket(id, body.Resolution),
            clock.GetUtcNow());
        stream.AppendOne(evt);
        await session.SaveChangesAsync(ct);
        return Results.NoContent();
    }

    [WolverineGet("/tickets/{id:guid}")]
    public static async Task<IResult> Get(
        Guid id,
        IQuerySession session,
        CancellationToken ct)
    {
        var summary = await session.LoadAsync<TicketSummary>(id, ct);
        return summary is null ? Results.NotFound() : Results.Ok(summary);
    }

    [WolverineGet("/tickets")]
    public static async Task<IResult> List(
        [FromQuery] string? status,
        IQuerySession session,
        CancellationToken ct)
    {
        var query = session.Query<TicketSummary>();
        if (string.IsNullOrWhiteSpace(status))
        {
            var allResults = await query.ToListAsync(ct);
            return Results.Ok(allResults);
        }

        if (!Enum.TryParse<TicketStatus>(status, ignoreCase: true, out var parsedStatus))
        {
            return Results.BadRequest(new
            {
                error = TicketEndpointConstants.InvalidStatusMessage(status)
            });
        }

        var results = await query.Where(t => t.Status == parsedStatus).ToListAsync(ct);
        return Results.Ok(results);
    }

    [WolverinePost("/tickets/{id:guid}/close")]
    public static async Task<IResult> Close(
        Guid id,
        [FromBody] CloseTicketBody body,
        IDocumentSession session,
        TimeProvider clock,
        CancellationToken ct)
    {
        var stream = await session.Events.FetchForWriting<Ticket>(id, ct);
        if (stream.Aggregate is null) return Results.NotFound();

        var evt = TicketDecider.Close(
            stream.Aggregate,
            new CloseTicket(id, body.ClosedBy),
            clock.GetUtcNow());
        stream.AppendOne(evt);
        await session.SaveChangesAsync(ct);
        return Results.NoContent();
    }
}

public record AssignTicketBody(string Assignee);
public record ResolveTicketBody(string Resolution);
public record CloseTicketBody(string ClosedBy);
