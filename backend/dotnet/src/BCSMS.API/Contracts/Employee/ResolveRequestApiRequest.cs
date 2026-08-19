namespace BCSMS.API.Contracts.Employee;

/// <summary>
/// API contract for resolving a service request by an assigned employee.
/// </summary>
public record ResolveRequestApiRequest(string? Note = null);
