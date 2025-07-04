using Avalonia.Controls;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Utils;
using Utils.DebugCommands;

namespace Client_PSL.ViewModels;

public partial class ErrorViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _errorMessage = "Unknown error...";

    public ErrorViewModel()
    {

    }

    public ErrorViewModel(string errorMessage)
    {
        _errorMessage = errorMessage;
    }
}
