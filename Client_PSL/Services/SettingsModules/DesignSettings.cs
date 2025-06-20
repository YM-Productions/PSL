using System;
using System.ComponentModel;
using System.Reflection;
using System.Text.Json;
using Avalonia;
using Avalonia.Media;
using Utils;

namespace Client_PSL.Services.Settings;

public class DesignSettings : INotifyPropertyChanged, ISettingsModule
{
    private Color _highlightColor = Color.Parse("#ff8066");
    public Color HighlightColor
    {
        get => _highlightColor;
        set
        {
            if (_highlightColor != value)
            {
                _highlightColor = value;
                OnPropertyChanged(nameof(HighlightColor));

                if (Application.Current?.Resources != null)
                {
                    Application.Current.Resources[nameof(HighlightColor)] = HighlightColor;
                }
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void Load()
    {
        Debug.Log("Loading design settings...");

        if (Globals.fileService.ReadText(nameof(DesignSettings)) is string json &&
            JsonSerializer.Deserialize<DesignSettings>(json, ISettings.JsonOptions) is DesignSettings designSettings)
        {
            Debug.Log("Design settings loaded successfully.");

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

    public void Save()
    {
        Debug.Log("Saving design settings...");

        string json = JsonSerializer.Serialize(this, ISettings.JsonOptions);
        Globals.fileService.WriteText(nameof(DesignSettings), json);
    }
}
