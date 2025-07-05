using Avalonia;
using System;
using System.Linq;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using SpacetimeDB.Types;
using Networking.SpacetimeController;
using Client_PSL.Services;
using Utils;

namespace Client_PSL.ViewModels;

/// <summary>
/// Represents a helper class for opening views by associating a specific ViewModel type.
/// Provides the name of the view by removing the "ViewModel" suffix from the type name.
/// </summary>
public class ViewOpener
{
    /// <summary>
    /// Gets or sets the type of the ViewModel associated with the view to be opened.
    /// </summary>
    public Type ViewModelType;

    /// <summary>
    /// Gets the name of the view, derived from the ViewModel type name by removing the "ViewModel" suffix.
    /// </summary>
    public string Name { get => ViewModelType.Name.Replace("ViewModel", ""); }

    /// <summary>
    /// Initializes a new instance of the <see cref="ViewOpener"/> class with the specified ViewModel type.
    /// </summary>
    /// <param name="viewModelType">The type of the ViewModel to associate with this view opener.</param>
    public ViewOpener(Type viewModelType)
    {
        ViewModelType = viewModelType;
    }
}

/// <summary>
/// The main view model for the application's main window. 
/// Manages navigation between pages, tracks landing state, and provides access to global view models.
/// Also manages a collection of available view openers for dynamic view navigation.
/// </summary>
public partial class MainViewModel : ViewModelBase
{
    /// <summary>
    /// Gets the global debug page view model instance.
    /// </summary>
    public DebugViewModel DebugPage => Globals.debugViewModel;

    /// <summary>
    /// Gets a new instance of the settings page view model.
    /// </summary>
    public SettingsViewModel SettingsPage => new();

    /// <summary>
    /// Gets the collection of available view openers for dynamic view navigation.
    /// </summary>
    public static ObservableCollection<ViewOpener> ViewOpeners { get; } = new();

    /// <summary>
    /// Gets or sets the currently displayed main page view model.
    /// </summary>
    [ObservableProperty]
    private ViewModelBase? _mainPage;

    /// <summary>
    /// Gets or sets a value indicating whether the user has landed (is authenticated or on the landing page).
    /// </summary>
    [ObservableProperty]
    private bool _isLanded = false;

    /// <summary>
    /// Gets or sets a value indicating whether the user is not landed (not authenticated or not on the landing page).
    /// </summary>
    [ObservableProperty]
    private bool _isNotLanded = true;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainViewModel"/> class.
    /// Sets the initial page to the login page.
    /// </summary>
    public MainViewModel()
    {
        // _debugPage = Globals.debugViewModel;
        SetLoginPage();
    }

    /// <summary>
    /// Adds a new view opener for the specified ViewModel type to the collection, if not already present.
    /// Throws an exception if the type does not inherit from <see cref="ViewModelBase"/>.
    /// </summary>
    /// <param name="viewModelType">The type of the ViewModel to add as a view opener.</param>
    public static void AddViewOpener(Type viewModelType)
    {
        if (!typeof(ViewModelBase).IsAssignableFrom(viewModelType))
            throw new ArgumentException($"Type {viewModelType.Name} is not a ViewModelBase type.");

        if (ViewOpeners.Where(v => v.ViewModelType == viewModelType).Any())
            return;

        ViewOpeners.Add(new ViewOpener(viewModelType));
    }

    /// <summary>
    /// Toggles the active state of the debug page.
    /// </summary>
    public void ToggleDebug()
    {
        DebugPage.IsActive = !DebugPage.IsActive;
    }

    /// <summary>
    /// Sets the main page to the login page view model.
    /// </summary>
    public void SetLoginPage()
    {
        MainPage = new LoginViewModel();
    }

    /// <summary>
    /// Sets the main page to the register page view model.
    /// </summary>
    public void SetRegisterPage()
    {
        MainPage = new RegisterViewModel();
    }

    /// <summary>
    /// Sets the landing state and updates the corresponding properties.
    /// </summary>
    public void SetLandingPage()
    {
        // MainPage = Globals.landingPageViewModel;
        IsLanded = true;
        IsNotLanded = false;
    }
}
