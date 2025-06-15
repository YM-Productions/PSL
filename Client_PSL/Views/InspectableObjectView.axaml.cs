using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Client_PSL.ViewModels;
using Utils;

namespace Client_PSL.Views;

/// <summary>
/// Represents a user control for displaying and interacting with an inspectable object in the UI.
/// This control is defined in XAML and its logic is implemented in this code-behind class.
/// </summary>
public partial class InspectableObjectView : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InspectableObjectView"/> class.
    /// Sets up the user interface components defined in the associated XAML file.
    /// </summary>
    public InspectableObjectView()
    {
        InitializeComponent();
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {

    }

    private void OnCopyButtonClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button &&
            button.DataContext is InspectableProperty property)
        {
            QuickUtils.SetClipboard(this, property.Value);
        }
    }
}
