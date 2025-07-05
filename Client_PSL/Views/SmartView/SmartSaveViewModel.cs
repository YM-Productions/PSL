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

public partial class SmartSaveViewModel : ViewModelBase
{
    public string DefaultConfigName { get => ISettings.Data.SmartView.DefaultConfigName; }

    [ObservableProperty]
    private string _selectedConfig = ISettings.Data.SmartView.DefaultConfigName;

    [ObservableProperty]
    private ObservableCollection<string> _configurations = new();

    [ObservableProperty]
    private string _newNameErrorText = string.Empty;

    public SmartSaveViewModel()
    {
        InitializeConfigruationNames();

        ISettings.Data.SmartView.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(ISettings.Data.SmartView.DefaultConfigName))
                OnPropertyChanged(nameof(DefaultConfigName));
        };
    }

    public void InitializeConfigruationNames()
    {
        Configurations.Clear();

        if (Globals.fileService.ReadText(SmartViewHost.SaveName) is not string json)
            return;

        Dictionary<string, object> savedViews = JsonSerializer.Deserialize<Dictionary<string, object>>(json, ISettings.JsonOptions) ?? new();
        foreach (string key in savedViews.Keys)
            Configurations.Add(key);
    }

    public void CreateNewConfig(string configName)
    {
        if (Configurations.Contains(configName))
        {
            NewNameErrorText = "Configuration name already exists.";
            return;
        }

        InitializeConfigruationNames();
    }
}
