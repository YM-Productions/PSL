using System.Reflection;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Client_PSL.ViewModels;

namespace Client_PSL.Views;

public partial class LoginView : UserControl
{
    public LoginView()
    {
        InitializeComponent();
    }

    private void OnLoginClick(object? sender, RoutedEventArgs e)
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
