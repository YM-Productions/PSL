using Utils;

namespace Client_PSL.Services.Settings;

/// <summary>
/// Represents the root application settings container.
/// </summary>
/// <remarks>
/// This class serves as the main entry point for accessing all application-wide settings.
/// It typically contains references to other settings modules, such as design, user, or feature settings.
/// Extend this class to include additional settings modules as needed.
/// </remarks>
public class AppSettings
{
    /// <summary>
    /// Gets or sets the design-related settings for the application.
    /// </summary>
    /// <remarks>
    /// The <see cref="Design"/> property provides access to UI appearance and theme configuration.
    /// </remarks>
    public DesignSettings Design { get; set; } = new();

    /// <summary>
    /// Gets or sets the SmartView settings for the application.
    /// This property provides access to the configuration options related to the SmartView feature,
    /// allowing customization and persistence of user preferences for SmartView functionality.
    /// </summary>
    public SmartViewSettings SmartView { get; set; } = new();
}

