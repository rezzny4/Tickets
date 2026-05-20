namespace Tickets.Api.Domain;

public static class TicketDecider
{
    public static TicketOpened Open(OpenTicket cmd, DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(cmd.Title))
            throw new InvalidTicketCommandException("Title is required.");

        return new TicketOpened(cmd.Title, cmd.Description ?? string.Empty, now);
    }

    public static TicketAssigned Assign(Ticket ticket, AssignTicket cmd, DateTimeOffset now)
    {
        if (ticket.Status is not (TicketStatus.Open or TicketStatus.Assigned))
            throw new InvalidTicketCommandException(
                $"Cannot assign a ticket in status {ticket.Status}.");

        if (string.IsNullOrWhiteSpace(cmd.Assignee))
            throw new InvalidTicketCommandException("Assignee is required.");

        return new TicketAssigned(cmd.Assignee, now);
    }

    public static TicketResolved Resolve(Ticket ticket, ResolveTicket cmd, DateTimeOffset now)
    {
        if (ticket.Status != TicketStatus.Assigned)
            throw new InvalidTicketCommandException(
                $"Cannot resolve a ticket in status {ticket.Status}; ticket must be Assigned.");

        if (string.IsNullOrWhiteSpace(cmd.Resolution))
            throw new InvalidTicketCommandException("Resolution is required.");

        return new TicketResolved(cmd.Resolution, now);
    }
}

public class InvalidTicketCommandException(string message) : Exception(message);
