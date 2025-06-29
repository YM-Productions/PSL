using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Client_PSL.ViewModels;
using Utils;

namespace Client_PSL.Views;

/// <summary>
/// Represents the modular browser view user control, which provides a UI for browsing and interacting
/// with collections of objects in a modular fashion. This control is defined in XAML and its logic is
/// implemented in this code-behind class.
/// </summary>
public partial class ModularBrowserView : UserControl
{
    Logger logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ModularBrowserView"/> class.
    /// Sets up the user interface components defined in the associated XAML file.
    /// </summary>
    public ModularBrowserView()
    {
        InitializeComponent();

        logger = Logger.LoggerFactory.GetLogger("ModularBrowser");
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        logger.Log("ModularBrowserView loaded");
    }

    private void OnTypeSelected(object? sender, RoutedEventArgs e)
    {
        logger.Log("Type selected in ModularBrowserView");
        if (sender is Button button &&
            button.DataContext is SelectableBrowserItem item &&
            DataContext is ModularBrowserViewModel viewModel)
            viewModel.SelectType(item.type);
    }

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        logger.Log("Selection changed in ModularBrowserView");

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
            logger.Log($"Search initiated with prompt: {prompt}");

            viewModel.Page = 0;
            viewModel.BrowseByIdentifier(prompt);

            e.Handled = true;
        }
    }

    private void OnNextPageButtonClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ModularBrowserViewModel viewModel &&
            SearchBox.Text is string prompt)
        {
            logger.Log($"Navigating to next page with prompt: {prompt}");

            viewModel.Page++;
            viewModel.BrowseByIdentifier(prompt);
        }
    }

    private void OnPreviousPageButtonClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ModularBrowserViewModel viewModel && viewModel.Page > 0 &&
                SearchBox.Text is string prompt)
        {
            logger.Log($"Navigating to previous page with prompt: {prompt}");

            viewModel.Page--;
            viewModel.BrowseByIdentifier(prompt);
        }
    }
}
