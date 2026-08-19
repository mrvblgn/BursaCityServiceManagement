namespace BCSMS.Application.ServiceRequests.Manager.Close;

public record CloseRequestCommand(
    Guid RequestId,
    string? Note,
    Guid ManagerUserId);
