namespace BCSMS.Application.ServiceRequests.Manager.Reopen;

public record ReopenRequestCommand(
    Guid RequestId,
    string? Note,
    Guid ManagerUserId);
