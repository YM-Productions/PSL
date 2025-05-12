using CommunityToolkit.Mvvm.ComponentModel;

using System;
using System.Linq;
using SpacetimeDB;
using SpacetimeDB.Types;
using System.Collections.Concurrent;
using System.Threading;

namespace Client_PSL.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    DebugViewModel _debugPage = new();

    public void ToggleDebug()
    {
        DebugPage.IsActive = !DebugPage.IsActive;
    }
}
