using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using System.Linq;
using System.Text.RegularExpressions;
using Client_PSL.ViewModels;
using Client_PSL.Services;
using Utils;

namespace Client_PSL.Views;

public partial class SmartSaveView : UserControl
{
    private Logger logger = Logger.LoggerFactory.CreateLogger(nameof(SmartSaveView));

    public SmartSaveView()
    {
        InitializeComponent();
        logger.Log("Initialized SmartSaveView");
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (DataContext is SmartSaveViewModel viewModel)
            ConfigGrid.SelectedItem = viewModel.DefaultConfigName;
        logger.Log("SmartSaveView loaded");
    }

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is DataGrid dg &&
            dg.SelectedItem is string configName &&
            DataContext is SmartSaveViewModel viewModel)
        {
            viewModel.SelectedConfig = configName;
        }
    }

    private void OnSaveButtonClick(object? sender, RoutedEventArgs e)
    {
        if (ConfigGrid.SelectedItem is string item)
            Globals.smartViewHost.SaveConfig(item);
    }

    private void OnLoadButtonClick(object? sender, RoutedEventArgs e)
    {
        if (ConfigGrid.SelectedItem is string item)
            Globals.smartViewHost.LoadConfig(item);
    }

    private void OnNewNameKeyDown(object? sender, KeyEventArgs e)
    {
        if (sender is TextBox tb &&
            tb.Text is string newName &&
            e.Key == Key.Enter &&
            DataContext is SmartSaveViewModel viewModel)
        {
            viewModel.NewNameErrorText = string.Empty;

            if (!Regex.IsMatch(newName, @"^[a-zA-Z0-9]+$"))
            {
                logger.Log("Invalid configuration name: " + newName);
                viewModel.NewNameErrorText = "Configuration name must contain only letters and numbers.";
                return;
            }

            viewModel.CreateNewConfig(newName);
        }
    }

    private void OnSetDefaultButtonClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is SmartSaveViewModel viewModel)
        {
            ISettings.Data.SmartView.DefaultConfigName = viewModel.SelectedConfig;
        }
    }

    private void OnDeleteButtonClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is SmartSaveViewModel viewModel)
            viewModel.DeleteSelectedConfig();
    }
}
