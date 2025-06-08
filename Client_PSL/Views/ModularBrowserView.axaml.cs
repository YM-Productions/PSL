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
    }

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is DataGrid dg &&
            dg.SelectedItem is MyObj obj)
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
