using CommunityToolkit.Mvvm.ComponentModel;

using SpacetimeDB;
using SpacetimeDB.Types;
using System.Collections.Concurrent;

namespace Client_PSL.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _greeting = "Welcome to Avalonia!";

    Identity? local_identity = null;
    ConcurrentQueue<(string Command, string Args)> input_queue = new();

    public MainViewModel()
    {

    }
}
