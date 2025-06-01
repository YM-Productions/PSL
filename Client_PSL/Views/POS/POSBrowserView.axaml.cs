using Avalonia.Controls;
using Avalonia.Interactivity;
using Utils;
using Client_PSL.ViewModels;

namespace Client_PSL.Views;

public partial class POSBrowserView : UserControl
{
    public POSBrowserView()
    {
        InitializeComponent();
    }

    private void OnButtonClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is POSBrowserViewModel vm)
        {
            vm.BrowsePhysicalObjects();
        }
    }

    private void OnButtonClickCreate(object? sender, RoutedEventArgs e)
    {
        if (DataContext is POSBrowserViewModel vm)
        {
            vm.CreatePhysicalObject();
        }
    }
}
