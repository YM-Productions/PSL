using Avalonia;
using Avalonia.Media;
using System;
using System.Linq;
using System.Reflection;
using Client_PSL.ViewModels;
using Client_PSL.Controls;
using Networking.SpacetimeController;
using Utils;

namespace Client_PSL.Services;

/// <summary>
/// Provides globally accessible methods and properties for application-wide operations and shared resources.
/// <para>
/// The <c>Globals</c> static class is intended to centralize functionality and data that need to be available
/// throughout the application, such as resource initialization, configuration, and other utility methods.
/// </para>
/// </summary>
public static class Globals
{
    public static IFileService fileService { get; set; } = null;

    public static MainViewModel mainViewModel { get; } = new();
    public static DebugViewModel debugViewModel { get; } = new();
    public static SpacetimeController spacetimeController { get; } = new();
    public static LandingPageViewModel landingPageViewModel { get; } = new();
    public static SmartViewHost smartViewHost { get; set; } = null;

    static Globals()
    {
        // Setup all Selectable Views in ViewOpener
        MainViewModel.AddViewOpener(typeof(ModularBrowserViewModel));
        MainViewModel.AddViewOpener(typeof(SmartSaveViewModel));

        // Setup all ignored Views for saving in SmartViewHost
        SmartViewHost.RegisterIgnoredView(typeof(SmartSaveViewModel));
    }

    /// <summary>
    /// Initializes application-wide resources by updating the resource dictionary of the current application
    /// with color properties defined in the design settings.
    /// <para>
    /// This method iterates through all properties of the <c>ISettings.Data.Design</c> object that are of type <see cref="Color"/>,
    /// and have both getter and setter accessors. For each such property, it sets or updates the corresponding entry
    /// in the application's <see cref="Application.Resources"/> dictionary, using the property name as the key and the
    /// property's value as the resource value.
    /// </para>
    /// <para>
    /// This allows dynamic theming or color customization at runtime, ensuring that any UI elements bound to these
    /// resource keys will automatically reflect the updated color values.
    /// </para>
    /// </summary>
    public static void InitializeResources()
    {
        if (Application.Current is Application app)
        {
            foreach (PropertyInfo prop in ISettings.Data.Design.GetType().GetProperties().Where(p => p.CanRead && p.CanWrite && p.PropertyType == typeof(Color)))
                app.Resources[prop.Name] = prop.GetValue(ISettings.Data.Design);
        }
    }
}
