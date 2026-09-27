namespace RegScope.Services;

/// <summary>Displays application messages and asks for deletion confirmation.</summary>
public interface IDialogService
{
    /// <summary>Shows an error message.</summary>
    void ShowError(string message, string title);

    /// <summary>Shows a warning message.</summary>
    void ShowWarning(string message, string title);

    /// <summary>Asks for confirmation, with rejection selected by default.</summary>
    bool Confirm(string message, string title);
}
