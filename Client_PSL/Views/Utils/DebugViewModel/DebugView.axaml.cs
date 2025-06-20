using Avalonia.Controls;
using Avalonia.Input;
using Utils;
using Client_PSL.ViewModels;

namespace Client_PSL.Views;

/// <summary>
/// Represents the debug view user control, which provides a UI for displaying debugging information
/// within the application. This control is defined in XAML and its logic is implemented in this code-behind.
/// </summary>
public partial class DebugView : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DebugView"/> class.
    /// Sets up the user interface components defined in the associated XAML file.
    /// </summary>
    public DebugView()
    {
        InitializeComponent();
    }

    private void Input_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter &&
            sender is TextBox textBox &&
            !string.IsNullOrEmpty(textBox.Text) &&
            DataContext is DebugViewModel vm)
        {
            vm.HanldeMessage(textBox.Text);
            textBox.Text = string.Empty;
            e.Handled = true;
        }
    }

    private void OnScrollInit(object? sender, System.EventArgs e)
    {
        if (sender is ScrollViewer scrollViewer &&
            DataContext is DebugViewModel vm)
        {
            vm.SetChatScrollViewer(scrollViewer);
        }
    }
}
