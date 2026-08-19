namespace BCSMS.Application.ServiceRequests.GetById;

/// <summary>
/// Query for retrieving a service request by ID on behalf of a requesting user.
/// </summary>
public record GetServiceRequestByIdQuery(
    Guid RequestId,
    Guid RequestingUserId);
