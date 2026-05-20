namespace Tickets.Api.Domain;

public static class TicketConstants
{
    public static class ErrorMessages
    {
        public const string TitleRequired = "Title is required.";
        public const string AssigneeRequired = "Assignee is required.";
        public const string ResolutionRequired = "Resolution is required.";
        public const string ClosedByRequired = "ClosedBy is required.";
        public const string CannotAssignInStatus = "Cannot assign a ticket in status {0}.";
        public const string CannotResolveInStatus = "Cannot resolve a ticket in status {0}; ticket must be Assigned.";
        public const string CannotCloseInStatus = "Cannot close a ticket in status {0}; ticket must be Resolved.";
    }
}
