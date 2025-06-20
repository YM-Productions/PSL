using System.Reflection;
using System.Text.Json;
using Client_PSL.Json;
using Client_PSL.Services.Settings;
using Utils;

namespace Client_PSL.Services;

public static class ISettings
{
    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
    };

    public static AppSettings Data { get; private set; } = new();

    static ISettings()
    {
        JsonOptions.Converters.Add(new AvaloniaColorJsonConverter());
    }

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

public interface ISettingsModule
{
    void Load();
    void Save();
}
