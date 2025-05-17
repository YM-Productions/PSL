using Avalonia.Controls;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Utils;
using Utils.DebugCommands;

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

    private bool _autScroll = true;

    private ScrollViewer? _scrollViewer = null;

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
    /// Sets the <see cref="ScrollViewer"/> instance used to display chat content.
    /// </summary>
    /// <param name="scrollViewer">
    /// The <see cref="ScrollViewer"/> instance that should be linked to the chat interface,
    /// typically for enabling auto-scrolling or programmatic scroll control.
    /// </param>
    /// <remarks>
    /// This method is used to inject the UI element responsible for displaying chat messages,
    /// allowing other parts of the application to scroll the view programmatically.
    /// </remarks>
    public void SetChatScrollViewer(ScrollViewer scrollViewer)
    {
        _scrollViewer = scrollViewer;
    }

    /// <summary>
    /// Toggles the auto-scroll behavior for the chat view and returns the updated state.
    /// </summary>
    /// <returns>
    /// <c>true</c> if auto-scroll is now enabled; <c>false</c> if it is disabled.
    /// </returns>
    /// <remarks>
    /// This method inverts the current <c>_autScroll</c> flag and returns the new state,
    /// allowing UI elements (e.g. a toggle button) to immediately reflect the change.
    /// </remarks>
    public bool ToggleAutoScroll()
    {
        _autScroll = !_autScroll;
        return _autScroll;
    }

    /// <summary>
    /// Explicitly sets whether auto-scroll should be enabled for the chat view.
    /// </summary>
    /// <param name="autoScroll">
    /// <c>true</c> to enable auto-scroll; <c>false</c> to disable it.
    /// </param>
    /// <remarks>
    /// When auto-scroll is enabled, the chat will automatically scroll to the newest message
    /// upon receiving new content.
    /// </remarks>
    public void SetAutoScroll(bool autoScroll)
    {
        _autScroll = autoScroll;
    }

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

        if (_scrollViewer != null &&
            _autScroll)
        {
            _scrollViewer.ScrollToEnd();
        }
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

        if (_scrollViewer != null &&
            _autScroll)
        {
            _scrollViewer.ScrollToEnd();
        }
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

    /// <summary>
    /// Handles incoming text input and dispatches it as a command if it starts with a forward slash.
    /// </summary>
    /// <param name="message">
    /// The raw input string, typically entered by the user. If it begins with <c>'/'</c>, it is treated as a command.
    /// </param>
    /// <remarks>
    /// <para>
    /// This method checks whether the input starts with <c>'/'</c>, which indicates a developer or debug command.
    /// If so, it strips the slash and forwards the command to <see cref="HandleCommand(string)"/> for parsing and execution.
    /// </para>
    /// <para>
    /// Messages that do not begin with <c>'/'</c> are ignored in this implementation.
    /// </para>
    /// </remarks>
    public void HanldeMessage(string message)
    {
        if (message.First() == '/')
        {
            // Handle Command
            HandleCommand(message.Substring(1));
        }
    }

    private void HandleCommand(string message)
    {
        // command layout: <command> <--attribute> <attribute value>
        string command = string.Empty;
        Dictionary<string, string> attributes = new();

        string[] parts = message.Split(" --", StringSplitOptions.None);

        for (int i = 0; i < parts.Length; i++)
        {
            if (i == 0)
            {
                command = parts[i].Trim();
            }
            else
            {
                if (attributes.ContainsKey(parts[i].Trim()))
                {
                    Debug.LogError($"You already used <{parts[i].Trim()}>");
                    return;
                }
                int spaceIndex = parts[i].IndexOf(' ');
                if (spaceIndex == -1)
                {
                    attributes[parts[i].Trim()] = string.Empty;
                }
                else
                {
                    string key = parts[i].Substring(0, spaceIndex).Trim();
                    string value = parts[i].Substring(spaceIndex + 1).Trim();
                    attributes[key] = value;
                }
            }
        }

        // Validate Command
        if (string.IsNullOrEmpty(command))
        {
            Debug.LogError("Command must not be empty!\nType </help> for help");
            return;
        }

        if (DebugCommand.Commands.TryGetValue(command.ToLower(), out Action<Dictionary<string, string>>? action) && action != null)
        {
            action(attributes);
        }
        else
        {
            Debug.LogError($"<{command}> is no valid command!");
            return;
        }
    }
}
