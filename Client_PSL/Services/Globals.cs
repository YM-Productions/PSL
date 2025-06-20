using Client_PSL.ViewModels;
using Networking.SpacetimeController;

namespace Client_PSL.Services;

public static class Globals
{
    public static IFileService fileService { get; set; } = null;

    public static MainViewModel mainViewModel { get; } = new();
    public static DebugViewModel debugViewModel { get; } = new();
    public static SpacetimeController spacetimeController { get; } = new();
    public static LandingPageViewModel landingPageViewModel { get; } = new();
}
