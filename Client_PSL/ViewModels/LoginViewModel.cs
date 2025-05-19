using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Networking.SpacetimeController;
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
    
    public async Task LoginAttempt()
    {
       await _spacetimeController.OpenTemporarySession();
        _spacetimeController.Login(_username, _password);
    }
    
    public async Task RegisterAttempt()
    {
        await _spacetimeController.OpenTemporarySession(); 
        _spacetimeController.Register(_username,"TESTMAIL@gmail.com", _password, false, true);
    }
}
