using Marten.Events;
using Marten.Events.Aggregation;
using Tickets.Api.Domain;

namespace Tickets.Api.Projections;

public record TicketSummary(
    Guid Id,
    string Title,
    string Status,
    string? Assignee,
    DateTimeOffset OpenedAt,
    DateTimeOffset? LastUpdatedAt);

public class TicketSummaryProjection : SingleStreamProjection<TicketSummary>
{
    public TicketSummary Create(IEvent<TicketOpened> @event) =>
        new(@event.StreamId, @event.Data.Title, nameof(TicketStatus.Open), null, @event.Data.OpenedAt, @event.Data.OpenedAt);

    public TicketSummary Apply(TicketAssigned @event, TicketSummary current) =>
        current with
        {
            Status = nameof(TicketStatus.Assigned),
            Assignee = @event.Assignee,
            LastUpdatedAt = @event.AssignedAt
        };

    public TicketSummary Apply(TicketResolved @event, TicketSummary current) =>
        current with
        {
            Status = nameof(TicketStatus.Resolved),
            LastUpdatedAt = @event.ResolvedAt
        };
}
