using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using System;
using System.Threading.Tasks;

namespace CLient_PSL.Style.Animations;

public static class UIEffects
{
    public static async Task PlaySnapEffect(Control control, double scale = 1.02, int durationMs = 80)
    {
        if (control.RenderTransform is not ScaleTransform scaleTransform)
        {
            scaleTransform = new ScaleTransform(1, 1);
            control.RenderTransform = scaleTransform;
        }

        scaleTransform.ScaleX = scale;
        scaleTransform.ScaleY = scale;

        await Task.Delay(durationMs);

        scaleTransform.ScaleX = 1;
        scaleTransform.ScaleY = 1;
    }
}
