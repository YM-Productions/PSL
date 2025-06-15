using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.VisualTree;
using Avalonia.Styling;
using Avalonia.Media;
using System;
using System.Threading.Tasks;

namespace Client_PSL;

/// <summary>
/// Provides an attached behavior to animate controls with a "pop-in" effect when they are loaded into the visual tree.
/// </summary>
public static class AnimateOnLoadBehavior
{
    /// <summary>
    /// Identifies the IsEnabled attached property.
    /// When set to true on a control, the pop-in animation will be triggered when the control is attached to the visual tree.
    /// </summary>
    public static readonly AttachedProperty<bool> IsEnabledProperty =
        AvaloniaProperty.RegisterAttached<Control, bool>(
            "IsEnabled",
            typeof(AnimateOnLoadBehavior),
            false);

    /// <summary>
    /// Static constructor. Registers a handler that listens for changes to the IsEnabled attached property.
    /// When enabled, attaches an event handler to the control's AttachedToVisualTree event to trigger the animation.
    /// </summary>
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

    /// <summary>
    /// Sets the IsEnabled attached property on the specified element.
    /// </summary>
    /// <param name="element">The element on which to set the property.</param>
    /// <param name="value">True to enable the pop-in animation; otherwise, false.</param>
    public static void SetIsEnabled(AvaloniaObject element, bool value) =>
        element.SetValue(IsEnabledProperty, value);

    /// <summary>
    /// Gets the value of the IsEnabled attached property from the specified element.
    /// </summary>
    /// <param name="element">The element from which to read the property.</param>
    /// <returns>True if the pop-in animation is enabled; otherwise, false.</returns>
    public static bool GetIsEnabled(AvaloniaObject element) =>
        element.GetValue(IsEnabledProperty);

    /// <summary>
    /// Runs a "pop-in" animation on the specified control.
    /// The animation fades in the control and scales it from 80% to 110% and then to 100% for a bouncy effect.
    /// </summary>
    /// <param name="control">The control to animate.</param>
    /// <returns>A task that completes when the animation is finished.</returns>
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
