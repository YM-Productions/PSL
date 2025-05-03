using Avalonia.Controls;
using Avalonia.Input;
using Client_PSL.ViewModels;

namespace Client_PSL.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }

    private void InputBox_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && sender is TextBox textBox
            && DataContext is MainViewModel mdl)
        {
            string input = textBox.Text ?? string.Empty;
            e.Handled = true;

            mdl.PricessInput(input);
        }
    }
}