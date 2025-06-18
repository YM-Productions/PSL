using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Networking.SpacetimeController;
using Client_PSL.Services;
using Utils;

namespace Client_PSL.ViewModels;

public partial class RegisterViewModel : ViewModelBase
{

    public RegisterViewModel()
    {
    }

    [ObservableProperty]
    private string _username = "";
    [ObservableProperty]
    private string _password = "";
    [ObservableProperty]
    private string _mail = "";
    [ObservableProperty]
    private bool _agb = false;
    [ObservableProperty]
    private bool _sendNews = true;

    [ObservableProperty]
    public object _errorMessage = "";

    public void RegisterAttempt()
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
            Globals.spacetimeController.Register(_username, _mail, _password, _sendNews, _agb);
            return Task.CompletedTask;
        });

    }

    public void switchToLogin()
    {
        Globals.mainViewModel.SetLoginPage();
    }
}
