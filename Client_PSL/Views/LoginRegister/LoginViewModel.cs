using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Networking.SpacetimeController;
using Client_PSL.Services;
using Utils;

namespace Client_PSL.ViewModels;

public partial class LoginViewModel : ViewModelBase
{
    public LoginViewModel()
    {
    }

    [ObservableProperty]
    private string _username = "";
    [ObservableProperty]
    private string _password = "";
    [ObservableProperty]
    public object _errorMessage = "";

    public void LoginAttempt()
    {
        Globals.spacetimeController.OpenTemporarySession();
        DateTime start = DateTime.Now;
        _ = Task.Run(() =>
        {
            while (!Globals.spacetimeController.IsConnected)
            {
                if (DateTime.Now.Subtract(start).TotalSeconds > 5)
                {
                    Debug.LogWarning("Server timed out");
                    return Task.CompletedTask;
                }

                Task.Delay(100);
            }
            Globals.spacetimeController.Login(_username, _password);
            return Task.CompletedTask;
        });
    }

    public void switchToRegister()
    {
        Globals.mainViewModel.SetRegisterPage();
    }
}
