using Avalonia.Controls;
using Avalonia.Input;
using Utils;
using Client_PSL.ViewModels;

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
            DebugViewModel.Instance.HanldeMessage(textBox.Text);
            textBox.Text = string.Empty;
            e.Handled = true;
        }
    }
}
