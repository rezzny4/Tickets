namespace Tickets.Api.Domain;

public record OpenTicket(string Title, string Description);

public record AssignTicket(Guid TicketId, string Assignee);

public record ResolveTicket(Guid TicketId, string Resolution);
