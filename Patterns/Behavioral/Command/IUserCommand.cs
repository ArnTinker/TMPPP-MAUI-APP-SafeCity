namespace SafeCity.Patterns.Behavioral.Command;

/// <summary>
/// PATTERN: Command — Command interface.
/// Justification: User actions (report, upvote, share, join network) are encapsulated
/// as objects implementing Execute/Undo. This enables an action history, decouples
/// the UI event from the business logic, and supports undo without if/else chains.
/// </summary>
public interface IUserCommand
{
    string Description { get; }
    Task ExecuteAsync();
    Task UndoAsync();
}
