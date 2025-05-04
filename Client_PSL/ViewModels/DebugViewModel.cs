using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Utils;

namespace Client_PSL.ViewModels;

/// <summary>
/// Represents a single debug log entry including timestamp, log level and message text.
/// This model is designed to be displayed in a UI with colored visual formatting per log part.
/// </summary>
public class DebugMessage
{
    private static readonly Color DEFAULT_COLOR = Colors.White;

    /// <summary>
    /// Gets or sets the background color of the entire log entry.
    /// Default is transparent.
    /// </summary>
    public IBrush BackgroundColor { get; set; } = new SolidColorBrush(Colors.Transparent);

    /// <summary>
    /// Gets or sets the timestamp of the log entry in string format.
    /// Typically formatted as "yyyy-MM-dd | HH:mm:ss".
    /// </summary>
    public string DateTime { get; set; }

    /// <summary>
    /// Gets or sets the brush color for the timestamp text.
    /// </summary>
    public IBrush DateTimeColor { get; set; } = new SolidColorBrush(DEFAULT_COLOR);

    /// <summary>
    /// Gets or sets the name of the log level, e.g., "INFO", "WARNING", "ERROR".
    /// </summary>
    public string LevelName { get; set; }

    /// <summary>
    /// Gets or sets the brush used to render the level name in the UI.
    /// </summary>
    public IBrush LevelColor { get; set; } = new SolidColorBrush(DEFAULT_COLOR);

    /// <summary>
    /// Gets or sets the actual log message.
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// Gets or sets the brush used to render the message text.
    /// </summary>
    public IBrush MessageColor { get; set; } = new SolidColorBrush(DEFAULT_COLOR);

    /// <summary>
    /// Initializes a new <see cref="DebugMessage"/> instance with date/time, level name, level color and message text.
    /// </summary>
    /// <param name="dateTime">The timestamp of the log message.</param>
    /// <param name="levelName">The log level name (e.g., "ERROR").</param>
    /// <param name="levelColor">The color representing the log level.</param>
    /// <param name="message">The main log message text.</param>
    public DebugMessage(string dateTime, string levelName, Color levelColor, string message)
    {
        DateTime = dateTime;
        LevelName = levelName;
        Message = message;

        LevelColor = new SolidColorBrush(levelColor);
    }
}

/// <summary>
/// ViewModel that holds and manages a list of <see cref="DebugMessage"/> entries
/// for use in a debug console or UI log viewer. Automatically trims message list
/// to avoid unbounded growth.
/// </summary>
public partial class DebugViewModel : ViewModelBase
{
    /// <summary>
    /// Singleton instance of the <see cref="DebugViewModel"/> class.
    /// Used for global access from static contexts.
    /// </summary>
    public static DebugViewModel Instance { get; private set; }

    [ObservableProperty]
    private bool _isActive = false;
    /// <summary>
    /// Maximum number of messages allowed in the <see cref="Messages"/> collection.
    /// Older messages will be trimmed if this limit is exceeded.
    /// </summary>
    public static readonly int MAX_MESSAGES = 100;

    [ObservableProperty]
    private ObservableCollection<DebugMessage> _messages = new();

    /// <summary>
    /// Initializes the singleton instance if not already set.
    /// </summary>
    public DebugViewModel()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else return;
    }

    /// <summary>
    /// Gets the current time as a formatted string.
    /// Format: yyyy-MM-dd | HH:mm:ss
    /// </summary>
    private static string dateTime => DateTime.Now.ToString("yyyy-MM-dd | HH:mm:ss");

    /// <summary>
    /// Adds a new debug message using given log level name, color, and message content.
    /// Timestamp is automatically generated.
    /// </summary>
    /// <param name="levelName">The log level name (e.g., "INFO").</param>
    /// <param name="levelColor">The color representing the log level.</param>
    /// <param name="message">The log message text.</param>
    public void AddMessage(string levelName, Color levelColor, string message)
    {
        AddMessage(new(dateTime, levelName, levelColor, message));
    }

    /// <summary>
    /// Adds a pre-constructed <see cref="DebugMessage"/> to the message list and trims if necessary.
    /// </summary>
    /// <param name="message">The debug message to add.</param>
    public void AddMessage(DebugMessage message)
    {
        _messages.Add(message);
        // Check if there are to many msges
        TrimMessages();
    }

    /// <summary>
    /// Removes the oldest message from the list if the maximum number of allowed messages is exceeded.
    /// </summary>
    private void TrimMessages()
    {
        if (Messages.Count > MAX_MESSAGES)
        {
            Messages.Remove(Messages.Last());
        }
    }
}
