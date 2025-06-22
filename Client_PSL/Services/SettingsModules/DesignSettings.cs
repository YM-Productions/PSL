using System;
using System.ComponentModel;
using System.Reflection;
using System.Text.Json;
using Avalonia;
using Avalonia.Media;
using Utils;

namespace Client_PSL.Services.Settings;

/// <summary>
/// Provides design and theme-related settings for the application, such as highlight color.
/// </summary>
/// <remarks>
/// This settings module manages UI appearance options. It supports property change notifications
/// for data binding and updates global Avalonia resources to enable dynamic theme changes at runtime.
/// </remarks>
public class DesignSettings : INotifyPropertyChanged, ISettingsModule
{
    private Logger logger = Logger.LoggerFactory.CreateLogger(nameof(ISettings));

    private Color _accentColor = Color.Parse("#ff8500");

    /// <summary>
    /// Gets or sets the application's highlight color.
    /// </summary>
    /// <remarks>
    /// When set, this property raises <see cref="PropertyChanged"/> and updates the global
    /// <c>HighlightColor</c> resource in <see cref="Application.Current.Resources"/>, so that
    /// all UI elements using this resource update automatically.
    /// </remarks>
    public Color AccentColor
    {
        get => _accentColor;
        set
        {
            if (_accentColor != value)
            {
                _accentColor = value;
                OnPropertyChanged(nameof(AccentColor));

                if (Application.Current?.Resources != null)
                {
                    Application.Current.Resources[nameof(AccentColor)] = AccentColor;
                }
            }
        }
    }

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event for the specified property.
    /// </summary>
    /// <param name="propertyName">The name of the property that changed.</param>
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <inheritdoc/>
    public void Load()
    {
        logger.Log("Loading design settings...");

        if (Globals.fileService.ReadText(nameof(DesignSettings)) is string json &&
            JsonSerializer.Deserialize<DesignSettings>(json, ISettings.JsonOptions) is DesignSettings designSettings)
        {
            logger.Log("Design settings loaded successfully.");

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
        logger.Log("Saving design settings...");

        string json = JsonSerializer.Serialize(this, ISettings.JsonOptions);
        Globals.fileService.WriteText(nameof(DesignSettings), json);
    }
}
