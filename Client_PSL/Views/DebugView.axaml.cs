using Avalonia.Controls;
using Avalonia.Input;
using Client_PSL.ViewModels;
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
            sender is TextBox textBox)
        {
            Debug.Log(textBox.Text ?? "-_-");
        }
    }
}