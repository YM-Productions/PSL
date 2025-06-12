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
        // Debug.Log(BrowserGrid.Tag.ToString());
        // Debug.Log(BrowserGrid.ItemsSource.GetType().Name);
    }

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is DataGrid dg &&
            dg.SelectedItem is BrowsableObj obj &&
            DataContext is ModularBrowserViewModel viewModel)
        {
            viewModel.SelectedName = obj.Name;
        }
    }

    private void OnButtonClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ModularBrowserViewModel viewModel)
        {
            viewModel.CreateBrowser();
        }
    }

    private void OnSearchKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && sender is TextBox textBox &&
            DataContext is ModularBrowserViewModel viewModel &&
            SearchBox.Text is string prompt)
        {
            viewModel.Page = 0;
            viewModel.BrowseByName(prompt, ParentFilter.Text);

            e.Handled = true;
        }
    }

    private void OnNextPageButtonClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ModularBrowserViewModel viewModel &&
            SearchBox.Text is string prompt)
        {
            viewModel.Page++;
            viewModel.BrowseByName(prompt, ParentFilter.Text);
        }
    }

    private void OnPreviousPageButtonClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ModularBrowserViewModel viewModel && viewModel.Page > 0 &&
                SearchBox.Text is string prompt)
        {
            viewModel.Page--;
            viewModel.BrowseByName(prompt, ParentFilter.Text);
        }
    }
}
