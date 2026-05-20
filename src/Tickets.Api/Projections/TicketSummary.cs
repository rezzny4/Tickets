using Marten.Events;
using Marten.Events.Aggregation;
using Tickets.Api.Domain;

namespace Tickets.Api.Projections;

public record TicketSummary(
    Guid Id,
    string Title,
    TicketStatus Status,
    string? Assignee,
    string? ClosedBy,
    DateTimeOffset OpenedAt,
    DateTimeOffset? LastUpdatedAt,
    DateTimeOffset? ClosedAt);

public class TicketSummaryProjection : SingleStreamProjection<TicketSummary>
{
    public TicketSummary Create(IEvent<TicketOpened> @event) =>
        new(
            @event.StreamId,
            @event.Data.Title,
            TicketStatus.Open,
            null,
            null,
            @event.Data.OpenedAt,
            @event.Data.OpenedAt,
            null);

    public TicketSummary Apply(TicketAssigned @event, TicketSummary current) =>
        current with
        {
            Status = TicketStatus.Assigned,
            Assignee = @event.Assignee,
            LastUpdatedAt = @event.AssignedAt
        };

    public TicketSummary Apply(TicketResolved @event, TicketSummary current) =>
        current with
        {
            Status = TicketStatus.Resolved,
            LastUpdatedAt = @event.ResolvedAt
        };

    public TicketSummary Apply(TicketClosed @event, TicketSummary current) =>
        current with
        {
            Status = TicketStatus.Closed,
            LastUpdatedAt = @event.ClosedAt,
            ClosedBy = @event.ClosedBy,
            ClosedAt = @event.ClosedAt
        };
}
