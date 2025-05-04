using Avalonia.Controls;
using Avalonia.Input;
using Utils;

namespace Client_PSL.Views;

public partial class DebugView : UserControl
{
    public DebugView()
    {
        InitializeComponent();
    }

    private void Input_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter &&
            sender is TextBox textBox &&
            !string.IsNullOrEmpty(textBox.Text))
        {
            Debug.Log(textBox.Text);
            Debug.LogWarning(textBox.Text);
            Debug.LogError(textBox.Text);
            textBox.Text = string.Empty;
            e.Handled = true;
        }
    }
}