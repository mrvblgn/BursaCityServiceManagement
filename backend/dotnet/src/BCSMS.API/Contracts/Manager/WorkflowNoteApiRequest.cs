namespace BCSMS.API.Contracts.Manager;

/// <summary>
/// API contract for workflow operations accepting an optional note (reject, close, reopen).
/// </summary>
public record WorkflowNoteApiRequest(string? Note = null);
