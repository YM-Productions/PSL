﻿using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Networking.SpacetimeController;
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
    public object _errorMessage= "";
    
    public void RegisterAttempt()
    {
        SpacetimeController.Instance.OpenTemporarySession();
        DateTime start = DateTime.Now;
        _ = Task.Run(() =>
        {
            while (!SpacetimeController.Instance.IsConnected)
            {
                if (DateTime.Now.Subtract(start).TotalSeconds > 5)
                {
                    Debug.LogWarning("Server timed out");
                    return Task.CompletedTask;
                }

                Task.Delay(100);
            }
            SpacetimeController.Instance.Register(_username,_mail, _password, _sendNews, _agb);
            return Task.CompletedTask;
        });
        
    }
    
    public void switchToLogin()
    {
        MainViewModel.Instance.SetLoginPage();
    }
}
