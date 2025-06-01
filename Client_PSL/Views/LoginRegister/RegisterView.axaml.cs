using Avalonia.Controls;
using Avalonia.Interactivity;
using Client_PSL.ViewModels;

namespace Client_PSL.Views;

public partial class RegisterView : UserControl
{
    public RegisterView()
    {
        InitializeComponent();
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
