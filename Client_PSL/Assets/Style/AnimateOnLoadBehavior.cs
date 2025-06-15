using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.VisualTree;
using Avalonia.Styling;
using Avalonia.Media;
using System;
using System.Threading.Tasks;

namespace Client_PSL;

public static class AnimateOnLoadBehavior
{
    public static readonly AttachedProperty<bool> IsEnabledProperty =
        AvaloniaProperty.RegisterAttached<Control, bool>(
            "IsEnabled",
            typeof(AnimateOnLoadBehavior),
            false);

    static AnimateOnLoadBehavior()
    {
        IsEnabledProperty.Changed.AddClassHandler<Control>((control, args) =>
        {
            control.AttachedToVisualTree += async (_, __) =>
            {
                if (GetIsEnabled(control))
                {
                    await RunFadeInAnimation(control);
                }
            };
        });
    }

    public static void SetIsEnabled(AvaloniaObject element, bool value) =>
        element.SetValue(IsEnabledProperty, value);

    public static bool GetIsEnabled(AvaloniaObject element) =>
        element.GetValue(IsEnabledProperty);

    private static async Task RunFadeInAnimation(Control control)
    {
        Animation animation = new()
        {
            Duration = TimeSpan.FromMilliseconds(300),
            Children = {
                new KeyFrame
                {
                    Cue = new Cue(0d),
                    Setters =
                    {
                        new Setter(Control.OpacityProperty, 0d),
                        new Setter(ScaleTransform.ScaleXProperty, 0.8),
                        new Setter(ScaleTransform.ScaleYProperty, 0.8)
                    }
                },
                new KeyFrame
                {
                    Cue = new Cue(0.7),
                    Setters =
                    {
                        new Setter(Control.OpacityProperty, 1d),
                        new Setter(ScaleTransform.ScaleXProperty, 1.025),
                        new Setter(ScaleTransform.ScaleYProperty, 1.025)
                    }
                },
                new KeyFrame
                {
                    Cue = new Cue(1d),
                    Setters =
                    {
                        new Setter(Control.OpacityProperty, 1d),
                        new Setter(ScaleTransform.ScaleXProperty, 1.0),
                        new Setter(ScaleTransform.ScaleYProperty, 1.0)
                    }
                }
            },
        };
        await animation.RunAsync(control, System.Threading.CancellationToken.None);
    }
}
