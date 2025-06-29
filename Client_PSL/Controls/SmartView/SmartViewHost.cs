using Avalonia;
using Avalonia.Controls;

namespace Client_PSL.Controls;

public class SmartViewHost : Canvas
{
    public SmartViewHost()
    {

    }

    public void AddSmartView(SmartView view, Point position)
    {
        Children.Add(view);
        SetLeft(view, position.X);
        SetTop(view, position.Y);
    }

    public void BringToFront(SmartView view)
    {
        Children.Remove(view);
        Children.Add(view);
    }
}
