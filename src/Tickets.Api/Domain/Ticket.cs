namespace Tickets.Api.Domain;

public class Ticket
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Assignee { get; set; }
    public string? Resolution { get; set; }
    public string? ClosedBy { get; set; }
    public TicketStatus Status { get; set; }

    public void Apply(TicketOpened e)
    {
        Title = e.Title;
        Description = e.Description;
        Status = TicketStatus.Open;
    }

    public void Apply(TicketAssigned e)
    {
        Assignee = e.Assignee;
        Status = TicketStatus.Assigned;
    }

    public void Apply(TicketResolved e)
    {
        Resolution = e.Resolution;
        Status = TicketStatus.Resolved;
    }

    public void Apply(TicketClosed e)
    {
        ClosedBy = e.ClosedBy;
        Status = TicketStatus.Closed;
    }
}
