using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Client_PSL.ViewModels;

namespace Client_PSL.Views;

public partial class LoginView : UserControl
{
    public LoginView()
    {
        InitializeComponent();
    }

    private void OnEnter(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter &&
            DataContext is LoginViewModel vm)
        {
            vm.LoginAttempt();
            e.Handled = true;
        }
    }

    private void OnLoginClick(object? sender, RoutedEventArgs routedEventArgs)
    {
        if (sender is Button && DataContext is LoginViewModel vm)
        {
            vm.LoginAttempt();
        }
    }

    private void OnSwitchToRegister(object? sender, RoutedEventArgs e)
    {
        if (sender is Button && DataContext is LoginViewModel vm)
        {
            vm.switchToRegister();
        }
    }
}
