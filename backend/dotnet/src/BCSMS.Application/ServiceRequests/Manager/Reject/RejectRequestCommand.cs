namespace BCSMS.Application.ServiceRequests.Manager.Reject;

public record RejectRequestCommand(
    Guid RequestId,
    string? Note,
    Guid ManagerUserId);
