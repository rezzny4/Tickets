using Tickets.Api.Domain;

namespace Tickets.Api.Endpoints;

public static class TicketEndpointConstants
{
    public static string InvalidStatusMessage(string status) =>
        $"Invalid ticket status '{status}'. Allowed values: {AllowedStatusValues}.";

    private static string AllowedStatusValues => string.Join(", ", Enum.GetNames<TicketStatus>());
}
