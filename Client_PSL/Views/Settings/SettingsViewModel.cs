using Avalonia.Controls;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Timers;
using System.Linq;
using System.Reflection;
using System.ComponentModel;
using System.Collections.ObjectModel;
using SpacetimeDB.Types;
using Networking.SpacetimeController;
using Client_PSL.Services;
using Utils;

namespace Client_PSL.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    private static readonly TimeSpan saveInterval = TimeSpan.FromSeconds(5);
    private static Timer saveTimer = new(saveInterval);

    /// <summary>
    /// Represents a single design-related setting within the application, such as a color or style property.
    /// <para>
    /// The <c>DesignSetting</c> class implements <see cref="INotifyPropertyChanged"/> to support data binding and
    /// automatic UI updates when property values change. It is typically used to encapsulate individual design
    /// settings that can be modified by the user, allowing for dynamic customization of the application's appearance.
    /// </para>
    /// </summary>
    public class DesignSetting : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public object Value
        {
            get
            {
                if (ISettings.Data.Design.GetType().GetProperty(Name)?.GetValue(ISettings.Data.Design) is Color color)
                    return color;
                else return Colors.White;
            }
            set
            {
                if (ISettings.Data.Design.GetType().GetProperty(Name)?.GetValue(ISettings.Data.Design) is Color color)
                {
                    saveTimer.Stop();
                    saveTimer.Start();

                    ISettings.Data.Design.GetType().GetProperty(Name)?.SetValue(ISettings.Data.Design, value);
                    OnPropertyChanged(Name);
                }
            }
        }

        public DesignSetting(string name)
        {
            Name = name;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    [ObservableProperty]
    private ObservableCollection<DesignSetting> _designElements;

    public SettingsViewModel()
    {
        DesignElements = new();
        foreach (PropertyInfo prop in ISettings.Data.Design.GetType().GetProperties().Where(p => p.CanWrite && p.CanRead && p.PropertyType == typeof(Color)))
            DesignElements.Add(new DesignSetting(prop.Name));

        saveTimer = new Timer(saveInterval);
        saveTimer.Elapsed += SaveSettings;
    }

    private void SaveSettings(object? source, ElapsedEventArgs e)
    {
        saveTimer.Stop();
        ISettings.Data.Design.Save();
    }
}
