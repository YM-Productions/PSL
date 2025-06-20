using System.Reflection;
using System.Text.Json;
using Client_PSL.Json;
using Client_PSL.Services.Settings;
using Utils;

namespace Client_PSL.Services;

/// <summary>
/// Provides static methods and properties for managing application settings,
/// including loading, saving, and serialization options.
/// </summary>
/// <remarks>
/// This static class acts as the central access point for all application settings.
/// It manages the serialization and deserialization of settings modules,
/// and provides global JSON serialization options, including custom converters.
/// </remarks>
public static class ISettings
{
    /// <summary>
    /// Gets the global <see cref="JsonSerializerOptions"/> used for serializing and deserializing settings.
    /// </summary>
    /// <remarks>
    /// This options instance includes custom converters (e.g., for Avalonia Color)
    /// and is configured for indented (pretty-printed) JSON output.
    /// </remarks>
    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
    };

    /// <summary>
    /// Gets the current application settings data.
    /// </summary>
    /// <remarks>
    /// The <see cref="Data"/> property holds the root settings object,
    /// which contains all settings modules for the application.
    /// </remarks>
    public static AppSettings Data { get; private set; } = new();

    static ISettings()
    {
        JsonOptions.Converters.Add(new AvaloniaColorJsonConverter());
    }

    /// <summary>
    /// Loads all settings modules by invoking their <see cref="ISettingsModule.Load"/> methods.
    /// </summary>
    /// <remarks>
    /// This method iterates through all properties of the <see cref="Data"/> object
    /// that implement <see cref="ISettingsModule"/> and calls their <c>Load</c> method.
    /// Logging is performed for each loaded module.
    /// </remarks>
    public static void Load()
    {
        Logger logger = Logger.LoggerFactory.CreateLogger(nameof(ISettings));
        logger.SetLevel(9);
        logger.Log("Loading settings...");

        foreach (PropertyInfo prop in Data.GetType().GetProperties())
        {
            if (prop.GetValue(Data) is ISettingsModule module)
            {
                module.Load();
                logger.Log($"Loaded settings module: {prop.Name}");
            }
        }
    }

    /// <summary>
    /// Saves all settings modules by invoking their <see cref="ISettingsModule.Save"/> methods.
    /// </summary>
    /// <remarks>
    /// This method iterates through all properties of the <see cref="Data"/> object
    /// that implement <see cref="ISettingsModule"/> and calls their <c>Save</c> method.
    /// Logging is performed for each saved module.
    /// </remarks>
    public static void Save()
    {
        Logger logger = Logger.LoggerFactory.CreateLogger(nameof(ISettings));
        logger.SetLevel(9);
        logger.Log("Saving settings...");

        foreach (PropertyInfo prop in Data.GetType().GetProperties())
        {
            if (prop.GetValue(Data) is ISettingsModule module)
            {
                module.Save();
                logger.Log($"Saved settings module: {prop.Name}");
            }
        }
    }
}

/// <summary>
/// Defines the interface for a settings module that supports loading and saving its state.
/// </summary>
public interface ISettingsModule
{
    /// <summary>
    /// Loads the settings module state from persistent storage.
    /// </summary>
    void Load();

    /// <summary>
    /// Saves the settings module state to persistent storage.
    /// </summary>
    void Save();
}
