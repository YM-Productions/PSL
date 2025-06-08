using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Client_PSL.ViewModels;

namespace Client_PSL.Views;

public partial class RegisterView : UserControl
{
    public RegisterView()
    {
        InitializeComponent();
    }
    
    private void OnEnter(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter &&
            DataContext is RegisterViewModel vm)
        {
            vm.RegisterAttempt();
            e.Handled = true;
        }
    }

    private void OnRegisterClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button && DataContext is RegisterViewModel vm)
        { 
            vm.RegisterAttempt();
        }
    }
    
    private void OnSwitchToLogin(object? sender, RoutedEventArgs e)
    {
        if (sender is Button && DataContext is RegisterViewModel vm)
        { 
            vm.switchToLogin();
        }
    }
    
}
