using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;

namespace Client_PSL.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _greeting = "Welcome to Avalonia!";
}
