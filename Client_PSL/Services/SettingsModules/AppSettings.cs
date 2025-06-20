using System.ComponentModel;
using Utils;

namespace Client_PSL.Services.Settings;

public class AppSettings
{
    public DesignSettings Design { get; set; } = new();
}

public class DesignSettings : INotifyPropertyChanged, ISettingsModule
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public void Load()
    {

    }

    public void Save()
    {

    }
}
