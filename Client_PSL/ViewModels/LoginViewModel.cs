using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Networking.SpacetimeController;
using Utils;

namespace Client_PSL.ViewModels;

public partial class LoginViewModel : ViewModelBase
{
    private readonly SpacetimeController _spacetimeController;
    public LoginViewModel()
    {
        _spacetimeController = new SpacetimeController();
    }
    
    [ObservableProperty]
    private string _username = "";
    [ObservableProperty]
    private string _password = "";
    [ObservableProperty]
    private string _verySeriousInfo= "";
    
    public void LoginAttempt()
    {
       _spacetimeController.OpenTemporarySession();
       DateTime start = DateTime.Now;
       _ = Task.Run(() =>
       {
           while (!_spacetimeController.IsConnected)
           {
               if (DateTime.Now.Subtract(start).TotalSeconds > 5)
               {
                   Debug.LogWarning("Server timed out");
                   return Task.CompletedTask;
               }

               Task.Delay(100);
           }
           _spacetimeController.Login(_username, _password);
           return Task.CompletedTask;
       });
    }
    
    public void RegisterAttempt()
    {
        _spacetimeController.OpenTemporarySession();
        DateTime start = DateTime.Now;
        _ = Task.Run(() =>
        {
            while (!_spacetimeController.IsConnected)
            {
                if (DateTime.Now.Subtract(start).TotalSeconds > 5)
                {
                    Debug.LogWarning("Server timed out");
                    return Task.CompletedTask;
                }

                Task.Delay(100);
            }
            _spacetimeController.Register(_username,"mungg@gmail.com", _password, false, true);
            return Task.CompletedTask;
        });
        
    }
}
