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

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
    }

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is DataGrid dg &&
            dg.SelectedItem is BrowsableObject obj &&
            DataContext is ModularBrowserViewModel viewModel)
        {
            viewModel.SelectObject(obj.inspectableObject);
        }
    }

    private void OnSearchKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && sender is TextBox textBox &&
            DataContext is ModularBrowserViewModel viewModel &&
            SearchBox.Text is string prompt)
        {
            viewModel.Page = 0;
            viewModel.BrowseByName(prompt);

            e.Handled = true;
        }
    }

    private void OnNextPageButtonClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ModularBrowserViewModel viewModel &&
            SearchBox.Text is string prompt)
        {
            viewModel.Page++;
            viewModel.BrowseByName(prompt);
        }
    }

    private void OnPreviousPageButtonClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ModularBrowserViewModel viewModel && viewModel.Page > 0 &&
                SearchBox.Text is string prompt)
        {
            viewModel.Page--;
            viewModel.BrowseByName(prompt);
        }
    }
}
