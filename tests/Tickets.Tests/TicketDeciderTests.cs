using Shouldly;
using Tickets.Api.Domain;
using Xunit;

namespace Tickets.Tests;

public class TicketDeciderTests
{
    private static readonly DateTimeOffset Now = new(2026, 5, 15, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Open_emits_TicketOpened()
    {
        var evt = TicketDecider.Open(new OpenTicket("Login broken", "500 on /login"), Now);

        evt.Title.ShouldBe("Login broken");
        evt.Description.ShouldBe("500 on /login");
        evt.OpenedAt.ShouldBe(Now);
    }

    [Fact]
    public void Open_rejects_blank_title()
    {
        Should.Throw<InvalidTicketCommandException>(() =>
            TicketDecider.Open(new OpenTicket("  ", "x"), Now));
    }

    [Fact]
    public void Assign_on_open_ticket_emits_TicketAssigned()
    {
        var ticket = Replay(new TicketOpened("t", "d", Now));

        var evt = TicketDecider.Assign(ticket, new AssignTicket(Guid.NewGuid(), "alice"), Now);

        evt.Assignee.ShouldBe("alice");
    }

    [Fact]
    public void Resolve_requires_assigned_status()
    {
        var ticket = Replay(new TicketOpened("t", "d", Now));

        Should.Throw<InvalidTicketCommandException>(() =>
            TicketDecider.Resolve(ticket, new ResolveTicket(Guid.NewGuid(), "fixed"), Now));
    }

    [Fact]
    public void Resolve_after_assign_succeeds()
    {
        var ticket = Replay(
            new TicketOpened("t", "d", Now),
            new TicketAssigned("alice", Now));

        var evt = TicketDecider.Resolve(ticket, new ResolveTicket(Guid.NewGuid(), "fixed"), Now);

        evt.Resolution.ShouldBe("fixed");
    }

    private static Ticket Replay(params object[] events)
    {
        var ticket = new Ticket();
        foreach (var e in events)
        {
            switch (e)
            {
                case TicketOpened o: ticket.Apply(o); break;
                case TicketAssigned a: ticket.Apply(a); break;
                case TicketResolved r: ticket.Apply(r); break;
            }
        }
        return ticket;
    }
}
