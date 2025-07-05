using Avalonia.Controls;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Linq;
using System.Collections.ObjectModel;
using Client_PSL.Services;
using Client_PSL.Controls;
using Utils;

namespace Client_PSL.ViewModels;

/// <summary>
/// ViewModel for managing the SmartSave functionality in the SmartView feature.
/// Handles user interactions, validation, and state management for saving SmartView configurations.
/// Integrates with the application's settings and supports property change notifications for data binding.
/// </summary>
public partial class SmartSaveViewModel : ViewModelBase
{
    private static Logger logger = Logger.LoggerFactory.CreateLogger(nameof(SmartSaveViewModel));

    /// <summary>
    /// Gets the name of the default SmartView configuration from the application settings.
    /// This property is used for data binding and reflects the current default configuration name.
    /// </summary>
    public string DefaultConfigName { get => ISettings.Data.SmartView.DefaultConfigName; }

    [ObservableProperty]
    private string _selectedConfig = ISettings.Data.SmartView.DefaultConfigName;

    [ObservableProperty]
    private ObservableCollection<string> _configurations = new();

    [ObservableProperty]
    private string _newNameErrorText = string.Empty;
    [ObservableProperty]
    private string _deleteErrorText = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="SmartSaveViewModel"/> class.
    /// Sets up configuration names and subscribes to property changes in the SmartView settings.
    /// Ensures that changes to the default configuration name are propagated to the view via property change notifications.
    /// </summary>
    public SmartSaveViewModel()
    {
        logger.SetLevel(50);

        InitializeConfigruationNames();

        ISettings.Data.SmartView.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(ISettings.Data.SmartView.DefaultConfigName))
                OnPropertyChanged(nameof(DefaultConfigName));
        };

        logger.Log("SmartSaveViewModel initialized.");
    }

    /// <summary>
    /// Initializes the list of available SmartView configuration names.
    /// Reads the saved configurations from persistent storage, deserializes them,
    /// and populates the <see cref="Configurations"/> collection with their names.
    /// </summary>
    public void InitializeConfigruationNames()
    {
        logger.Log("Initializing configuration names...");
        Configurations.Clear();

        if (Globals.fileService.ReadText(SmartViewHost.SaveName) is not string json)
        {
            Debug.LogWarning("Failed to read SmartView configurations from file.");
            return;
        }

        Dictionary<string, object> savedViews = JsonSerializer.Deserialize<Dictionary<string, object>>(json, ISettings.JsonOptions) ?? new();
        foreach (string key in savedViews.Keys)
            Configurations.Add(key);

        logger.Log($"Loaded {Configurations.Count} configurations from {SmartViewHost.SaveName}.");
    }

    /// <summary>
    /// Creates a new SmartView configuration with the specified name.
    /// Validates that the configuration name does not already exist, displays an error message if it does,
    /// and saves the new configuration using the SmartView host. Updates the configuration names after saving.
    /// </summary>
    /// <param name="configName">The name of the new configuration to create.</param>
    public void CreateNewConfig(string configName)
    {
        logger.Log($"Creating new configuration: {configName}");
        if (Configurations.Contains(configName))
        {
            logger.Log($"Configuration name '{configName}' already exists.");
            NewNameErrorText = "Configuration name already exists.";
            return;
        }

        Globals.smartViewHost.SaveConfig(configName);
        InitializeConfigruationNames();
        logger.Log($"Configuration '{configName}' created successfully.");
    }

    /// <summary>
    /// Deletes the currently selected SmartView configuration.
    /// Prevents deletion of the default configuration and displays an error message if attempted.
    /// Calls the SmartView host to perform the deletion and refreshes the list of configuration names.
    /// </summary>
    public void DeleteSelectedConfig()
    {
        logger.Log($"Attempting to delete configuration: {SelectedConfig}");
        DeleteErrorText = string.Empty;

        if (DefaultConfigName == SelectedConfig)
        {
            logger.Log("Cannot delete the default configuration.");
            DeleteErrorText = "Cannot delete the default configuration.";
            return;
        }

        Globals.smartViewHost.DeleteConfig(SelectedConfig);
        InitializeConfigruationNames();
        logger.Log($"Configuration '{SelectedConfig}' deleted successfully.");
    }
}
