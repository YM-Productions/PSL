using System;
using System.ComponentModel;
using System.Reflection;
using System.Text.Json;
using Avalonia;
using Avalonia.Media;
using Utils;

namespace Client_PSL.Services.Settings;

/// <summary>
/// Represents the settings module for the SmartView feature.
/// This class manages configuration options and user preferences related to SmartView,
/// supports property change notifications, and integrates with the application's settings infrastructure.
/// </summary>
public class SmartViewSettings : INotifyPropertyChanged, ISettingsModule
{
    private Logger logger = Logger.LoggerFactory.CreateLogger(nameof(ISettings));

    private string _defaultConfigName = "default";

    /// <summary>
    /// Gets or sets the name of the default SmartView configuration.
    /// When set, this property raises a property changed notification.
    /// Only alphabetic characters (a-z, A-Z) are allowed in the configuration name.
    /// </summary>
    public string DefaultConfigName
    {
        get => _defaultConfigName;
        set
        {
            if (_defaultConfigName != value)
            {
                _defaultConfigName = value;
                OnPropertyChanged(nameof(DefaultConfigName));
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <inheritdoc/>
    public void Load()
    {
        logger.Log("Loading SmartViewSettings settings...");

        if (Globals.fileService.ReadText(nameof(SmartViewSettings)) is string json &&
            JsonSerializer.Deserialize<SmartViewSettings>(json, ISettings.JsonOptions) is SmartViewSettings designSettings)
        {
            logger.Log("SmartViewSettings settings loaded successfully.");

            foreach (PropertyInfo prop in GetType().GetProperties())
            {
                if (prop.CanWrite)
                {
                    object? Value = prop.GetValue(designSettings);
                    prop.SetValue(this, Value);
                }
            }
        }
        else Save();
    }

    /// <inheritdoc/>
    public void Save()
    {
        logger.Log("Saving SmartViewSettings settings...");

        string json = JsonSerializer.Serialize(this, ISettings.JsonOptions);
        Globals.fileService.WriteText(nameof(SmartViewSettings), json);
    }
}
