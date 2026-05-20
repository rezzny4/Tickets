namespace Tickets.Api.Domain;

public record TicketOpened(string Title, string Description, DateTimeOffset OpenedAt);

public record TicketAssigned(string Assignee, DateTimeOffset AssignedAt);

public record TicketResolved(string Resolution, DateTimeOffset ResolvedAt);

public record TicketClosed(string ClosedBy, DateTimeOffset ClosedAt);
