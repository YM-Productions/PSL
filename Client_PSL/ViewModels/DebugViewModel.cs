using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Utils;

namespace Client_PSL.ViewModels;

public class DebugMessage
{
    private static readonly Color DEFAULT_COLOR = Colors.White;
    public IBrush BackgroundColor { get; set; } = new SolidColorBrush(Colors.Transparent);

    public string DateTime { get; set; }
    public IBrush DateTimeColor { get; set; } = new SolidColorBrush(DEFAULT_COLOR);

    public string LevelName { get; set; }
    public IBrush LevelColor { get; set; } = new SolidColorBrush(DEFAULT_COLOR);
    
    public string Message { get; set; }
    public IBrush MessageColor { get; set; } = new SolidColorBrush(DEFAULT_COLOR);

    public DebugMessage(string dateTime, string levelName, Color levelColor, string message)
    {
        DateTime = dateTime;
        LevelName = levelName;
        Message = message;

        LevelColor = new SolidColorBrush(levelColor);
    }
}

public partial class DebugViewModel : ViewModelBase
{
    public static DebugViewModel Instance { get; private set; }
    public static readonly int MAX_MESSAGES = 100;

    [ObservableProperty]
    private ObservableCollection<DebugMessage> _messages = new();

    public DebugViewModel()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else return;
    }

    private static string dateTime => DateTime.Now.ToString("yyyy-MM-dd | HH:mm:ss");
    public void AddMessage(string levelName, Color levelColor, string message)
    {
        AddMessage(new(dateTime, levelName, levelColor, message));
    }

    public void AddMessage(DebugMessage message)
    {
        _messages.Add(message);
        // Check if there are to many msges
        TrimMessages();
    }

    private void TrimMessages()
    {
        if (Messages.Count > MAX_MESSAGES)
        {
            Messages.Remove(Messages.Last());
        }
    }
}
