using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Client_PSL.ViewModels;
using Utils;

namespace Client_PSL.Views;

public partial class ModularBrowserView : UserControl
{
    public ModularBrowserView()
    {
        InitializeComponent();

        // BrowserGrid.Columns.Add(new DataGridTextColumn
        // {
        //     Header = "Name",
        //     Binding = new Avalonia.Data.Binding("Name")
        // });
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        Debug.Log(BrowserGrid.Tag.ToString());
        // Debug.Log(BrowserGrid.ItemsSource.GetType().Name);
    }

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is DataGrid dg &&
            dg.SelectedItem is BrowsableObj obj)
        {
            Debug.Log(obj.Name);
        }
    }

    private void OnButtonClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ModularBrowserViewModel viewModel)
        {
            viewModel.CreateBrowser();
        }
    }
}
