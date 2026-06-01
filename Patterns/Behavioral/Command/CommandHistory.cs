namespace SafeCity.Patterns.Behavioral.Command;

/// <summary>
/// PATTERN: Command — Invoker with undo history.
/// Justification: Holds the executed command stack so the app can offer undo of the
/// last user action (e.g. accidental upvote) without the ViewModel managing state.
/// </summary>
public class CommandHistory
{
    private readonly Stack<IUserCommand> _history = new();

    public bool CanUndo => _history.Count > 0;

    public async Task ExecuteAsync(IUserCommand command)
    {
        await command.ExecuteAsync();
        _history.Push(command);
    }

    public async Task UndoLastAsync()
    {
        if (_history.TryPop(out var command))
            await command.UndoAsync();
    }

    public void Clear() => _history.Clear();
}
