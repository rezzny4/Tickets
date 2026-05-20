namespace Tickets.Api.Domain;

public static class TicketDecider
{
    public static TicketOpened Open(OpenTicket cmd, DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(cmd.Title))
            throw new InvalidTicketCommandException(TicketConstants.ErrorMessages.TitleRequired);

        return new TicketOpened(cmd.Title, cmd.Description ?? string.Empty, now);
    }

    public static TicketAssigned Assign(Ticket ticket, AssignTicket cmd, DateTimeOffset now)
    {
        if (ticket.Status is not (TicketStatus.Open or TicketStatus.Assigned))
            throw new InvalidTicketCommandException(
                string.Format(TicketConstants.ErrorMessages.CannotAssignInStatus, ticket.Status));

        if (string.IsNullOrWhiteSpace(cmd.Assignee))
            throw new InvalidTicketCommandException(TicketConstants.ErrorMessages.AssigneeRequired);

        return new TicketAssigned(cmd.Assignee, now);
    }

    public static TicketResolved Resolve(Ticket ticket, ResolveTicket cmd, DateTimeOffset now)
    {
        if (ticket.Status != TicketStatus.Assigned)
            throw new InvalidTicketCommandException(
                string.Format(TicketConstants.ErrorMessages.CannotResolveInStatus, ticket.Status));

        if (string.IsNullOrWhiteSpace(cmd.Resolution))
            throw new InvalidTicketCommandException(TicketConstants.ErrorMessages.ResolutionRequired);

        return new TicketResolved(cmd.Resolution, now);
    }

    public static TicketClosed Close(Ticket ticket, CloseTicket cmd, DateTimeOffset now)
    {
        if (ticket.Status != TicketStatus.Resolved)
            throw new InvalidTicketCommandException(
                string.Format(TicketConstants.ErrorMessages.CannotCloseInStatus, ticket.Status));

        if (string.IsNullOrWhiteSpace(cmd.ClosedBy))
            throw new InvalidTicketCommandException(TicketConstants.ErrorMessages.ClosedByRequired);

        return new TicketClosed(cmd.ClosedBy, now);
    }
}

public class InvalidTicketCommandException(string message) : Exception(message);
