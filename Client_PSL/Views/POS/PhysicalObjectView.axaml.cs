using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Client_PSL.ViewModels;
using Utils;

namespace Client_PSL.Views;

public partial class PhysicalObjectView : UserControl
{
    public PhysicalObjectView()
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
        // Debug.Log(BrowserGrid.Tag.ToString());
        // Debug.Log(BrowserGrid.ItemsSource.GetType().Name);
    }
}
