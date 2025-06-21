using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Rendering;
using System;

namespace Client_PSL.Controls;

/// <summary>
/// A custom Avalonia control that displays a color wheel for selecting colors using HSV (Hue, Saturation, Value) model.
/// Users can interact with the hue ring and the inner triangle to pick a color, and the control exposes properties for
/// the selected color in various formats (ARGB, Hex, HSV).
/// </summary>
public class ColorWheelControl : Control
{
    /// <summary>
    /// Identifies the <see cref="SelectedColor"/> styled property.
    /// </summary>
    public static readonly StyledProperty<Color> SelectedColorProperty =
        AvaloniaProperty.Register<ColorWheelControl, Color>(nameof(SelectedColor));

    /// <summary>
    /// Gets or sets the currently selected color.
    /// Setting this property updates the HSV values and notifies property changes for related color properties.
    /// </summary>
    public Color SelectedColor
    {
        get => GetValue(SelectedColorProperty);
        set
        {
            if (value == GetValue(SelectedColorProperty))
                return;

            SetValue(SelectedColorProperty, value);

            ColorToHSV(value, out double h, out _saturation, out _value);

            if (_inputMode != InputMode.Sat)
                _hue = h;

            InvalidateVisual();

            RaisePropertyChanged(RProperty, default, R);
            RaisePropertyChanged(GProperty, default, G);
            RaisePropertyChanged(BProperty, default, B);
            RaisePropertyChanged(AProperty, default, A);
            RaisePropertyChanged(HexProperty, default, Hex);
            RaisePropertyChanged(HueProperty, default, Hue);
        }
    }

    /// <summary>
    /// Identifies the <see cref="R"/> direct property.
    /// </summary>
    public static readonly DirectProperty<ColorWheelControl, byte> RProperty =
        AvaloniaProperty.RegisterDirect<ColorWheelControl, byte>(
                nameof(R),
                o => o.R,
                (o, v) => o.R = v);

    /// <summary>
    /// Gets or sets the red channel of the selected color.
    /// </summary>
    public byte R
    {
        get => SelectedColor.R;
        set
        {
            SelectedColor = Color.FromArgb(SelectedColor.A, value, SelectedColor.G, SelectedColor.B);
            RaisePropertyChanged(RProperty, default, value);
        }
    }

    /// <summary>
    /// Identifies the <see cref="G"/> direct property.
    /// </summary>
    public static readonly DirectProperty<ColorWheelControl, byte> GProperty =
        AvaloniaProperty.RegisterDirect<ColorWheelControl, byte>(
                nameof(G),
                o => o.G,
                (o, v) => o.G = v);

    /// <summary>
    /// Gets or sets the green channel of the selected color.
    /// </summary>
    public byte G
    {
        get => SelectedColor.G;
        set
        {
            SelectedColor = Color.FromArgb(SelectedColor.A, SelectedColor.R, value, SelectedColor.B);
            RaisePropertyChanged(GProperty, default, value);
        }
    }

    /// <summary>
    /// Identifies the <see cref="B"/> direct property.
    /// </summary>
    public static readonly DirectProperty<ColorWheelControl, byte> BProperty =
        AvaloniaProperty.RegisterDirect<ColorWheelControl, byte>(
                nameof(B),
                o => o.B,
                (o, v) => o.B = v);

    /// <summary>
    /// Gets or sets the blue channel of the selected color.
    /// </summary>
    public byte B
    {
        get => SelectedColor.B;
        set
        {
            SelectedColor = Color.FromArgb(SelectedColor.A, SelectedColor.R, SelectedColor.G, value);
            RaisePropertyChanged(BProperty, default, value);
        }
    }

    /// <summary>
    /// Identifies the <see cref="A"/> direct property.
    /// </summary>
    public static readonly DirectProperty<ColorWheelControl, byte> AProperty =
        AvaloniaProperty.RegisterDirect<ColorWheelControl, byte>(
                nameof(A),
                o => o.A,
                (o, v) => o.A = v);

    /// <summary>
    /// Gets or sets the alpha (opacity) channel of the selected color.
    /// </summary>
    public byte A
    {
        get => SelectedColor.A;
        set
        {
            SelectedColor = Color.FromArgb(value, SelectedColor.R, SelectedColor.G, SelectedColor.B);
            RaisePropertyChanged(AProperty, default, value);
        }
    }

    /// <summary>
    /// Identifies the <see cref="Hex"/> direct property.
    /// </summary>
    public static readonly DirectProperty<ColorWheelControl, string?> HexProperty =
        AvaloniaProperty.RegisterDirect<ColorWheelControl, string?>(
                nameof(Hex),
                o => o.Hex,
                (o, v) => o.Hex = v ?? string.Empty);

    /// <summary>
    /// Gets or sets the selected color as a hexadecimal string in the format #AARRGGBB.
    /// </summary>
    public string Hex
    {
        get => $"#{SelectedColor.A:X2}{SelectedColor.R:X2}{SelectedColor.G:X2}{SelectedColor.B:X2}";
        set
        {
            if (value is { Length: 9 } && value.StartsWith("#") &&
                byte.TryParse(value.Substring(1, 2), System.Globalization.NumberStyles.HexNumber, null, out var a) &&
                byte.TryParse(value.Substring(3, 2), System.Globalization.NumberStyles.HexNumber, null, out var r) &&
                byte.TryParse(value.Substring(5, 2), System.Globalization.NumberStyles.HexNumber, null, out var g) &&
                byte.TryParse(value.Substring(7, 2), System.Globalization.NumberStyles.HexNumber, null, out var b))
            {
                SelectedColor = Color.FromArgb(a, r, g, b);
                RaisePropertyChanged(HexProperty, default, Hex);
            }
        }
    }

    /// <summary>
    /// Identifies the <see cref="Hue"/> direct property.
    /// </summary>
    public static readonly DirectProperty<ColorWheelControl, double> HueProperty =
        AvaloniaProperty.RegisterDirect<ColorWheelControl, double>(
                nameof(Hue),
                o => o.Hue,
                (o, v) => o.Hue = v);

    /// <summary>
    /// Gets or sets the hue component of the selected color (0-360).
    /// </summary>
    public double Hue
    {
        get => (int)Math.Round(_hue);
        set
        {
            _hue = Math.Clamp(value, 0, 360);
            SelectedColor = ColorFromHSV(_hue, _saturation, _value);
            RaisePropertyChanged(HueProperty, default, _hue);
            InvalidateVisual();
        }
    }

    private enum InputMode { None, Hue, Sat }
    private InputMode _inputMode = InputMode.None;

    private double _hue = 0;
    private double _saturation = 1;
    private double _value = 1;

    private Point? _selectionPoint;

    private Point _triangleTop;
    private Point _triangleLeft;
    private Point _triangleRight;

    /// <summary>
    /// Initializes a new instance of the <see cref="ColorWheelControl"/> class and attaches pointer event handlers.
    /// </summary>
    public ColorWheelControl()
    {
        this.PointerPressed += OnPointerPressed;
        this.PointerMoved += OnPointerMoved;
        this.PointerReleased += OnPointerReleased;
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        Point pos = e.GetPosition(this);
        Point center = new(Bounds.Width / 2, Bounds.Height / 2);
        double outerRadius = Math.Min(Bounds.Width, Bounds.Height) / 2 - 5;
        double innerRadius = outerRadius * 0.75;

        if (IsPointInTriangle(pos, _triangleTop, _triangleLeft, _triangleRight, out _, out _, out _))
            _inputMode = InputMode.Sat;
        else if (IsHueRing(pos, center, innerRadius, outerRadius))
            _inputMode = InputMode.Hue;

        e.Pointer.Capture(this);
        UpdateColorFromPosition(pos);
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            UpdateColorFromPosition(e.GetPosition(this));
        }
    }
    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _inputMode = InputMode.None;
        e.Pointer.Capture(null);
    }

    private void UpdateColorFromPosition(Point pos)
    {
        Point center = new(Bounds.Width / 2, Bounds.Height / 2);
        double outerRadius = Math.Min(Bounds.Width, Bounds.Height) / 2 - 5;
        double innerRadius = outerRadius * 0.75;

        switch (_inputMode)
        {
            case InputMode.Sat:
                Point clamped = ClampToTriangle(pos, _triangleTop, _triangleLeft, _triangleRight);

                IsPointInTriangle(clamped, _triangleTop, _triangleLeft, _triangleRight, out double u, out double v, out double w);

                double s = u;
                double vVal = u + w;
                _saturation = s;
                _value = vVal;
                SelectedColor = ColorFromHSV(_hue, _saturation, _value);
                _selectionPoint = clamped;
                InvalidateVisual();
                break;

            case InputMode.Hue:
                _hue = GetHueFromPosition(pos, center);
                SelectedColor = ColorFromHSV(_hue, _saturation, _value);
                InvalidateVisual();
                break;
        }
    }

    private double GetHueFromPosition(Point pos, Point center)
    {
        double dx = pos.X - center.X;
        double dy = pos.Y - center.Y;
        double angle = Math.Atan2(dy, dx) * 180 / Math.PI;
        angle = (angle + 450) % 360; // +90°, dann normalisieren
        return angle;
    }

    private Point[] GetRotatedTrianglePoints(Point center, double radius, double angleDeg)
    {
        Point[] points = new Point[3];
        for (int i = 0; i < points.Length; i++)
        {
            double angle = (angleDeg + i * 120 - 90) * Math.PI / 180;
            points[i] = new(
                center.X + radius * Math.Cos(angle),
                center.Y + radius * Math.Sin(angle)
            );
        }

        return points;
    }

    private Point InterpolateTrianglePoint(double s, double v, Point top, Point left, Point right)
    {
        double u = s;
        double w = v - s;
        double total = u + w;

        if (total > 1)
        {
            u /= total;
            w /= total;
        }

        double vv = 1 - u - w;

        return new(
            u * top.X + vv * left.X + w * right.X,
            u * top.Y + vv * left.Y + w * right.Y
        );
    }

    private Point ClampToTriangle(Point p, Point a, Point b, Point c)
    {
        if (IsPointInTriangle(p, a, b, c, out _, out _, out _))
            return p;

        Point closest = ClosestPointOnSegment(p, a, b);
        double minDist = DistanceSquared(p, closest);

        Point test = ClosestPointOnSegment(p, b, c);
        double dist = DistanceSquared(p, test);
        if (dist < minDist)
        {
            minDist = dist;
            closest = test;
        }

        test = ClosestPointOnSegment(p, c, a);
        dist = DistanceSquared(p, test);
        if (dist < minDist)
            closest = test;

        return closest;
    }

    private Point ClosestPointOnSegment(Point p, Point a, Point b)
    {
        Vector ab = b - a;
        Vector ap = p - a;
        double t = Vector.Dot(ap, ab) / ab.SquaredLength;
        t = Math.Clamp(t, 0, 1);
        return a + ab * t;
    }

    private double DistanceSquared(Point a, Point b)
    {
        double dx = a.X - b.X;
        double dy = a.Y - b.Y;
        return dx * dx + dy * dy;
    }

    /// <summary>
    /// Renders the color wheel, hue ring, SV triangle, and selection indicator.
    /// </summary>
    /// <param name="context">The drawing context.</param>
    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (Bounds.Width <= 1 || Bounds.Height <= 1)
            return;

        // HueRung
        Point center = new(Bounds.Width / 2, Bounds.Height / 2);
        double outerRadius = Math.Min(Bounds.Width, Bounds.Height) / 2 - 5;
        double innerRadius = outerRadius * 0.75;

        for (int i = 0; i < 360; i++)
        {
            double angle = (i - 90) * Math.PI / 180;
            Color color = ColorFromHSV(i, 1, 1);
            double strokeWidth = Math.Max(1, outerRadius * 0.02);
            Pen pen = new(new SolidColorBrush(color), strokeWidth);

            Point outer = center + new Vector(Math.Cos(angle), Math.Sin(angle)) * outerRadius;
            Point inner = center + new Vector(Math.Cos(angle), Math.Sin(angle)) * innerRadius;

            context.DrawLine(pen, outer, inner);
        }

        // SatTriangle
        double triangleRadius = innerRadius * 0.95;
        Point[] trianglePoints = GetRotatedTrianglePoints(center, triangleRadius, _hue);

        _triangleTop = trianglePoints[0];
        _triangleLeft = trianglePoints[1];
        _triangleRight = trianglePoints[2];

        _selectionPoint = InterpolateTrianglePoint(_saturation, _value, _triangleTop, _triangleLeft, _triangleRight);

        // context.DrawGeometry(Brushes.White, new Pen(Brushes.Black, 1), triangle);

        int triangleWidth = (int)Math.Ceiling(Bounds.Width);
        int triangleHeight = (int)Math.Ceiling(Bounds.Height);

        WriteableBitmap svTriangle = RenderSaturationValueTriangle(_triangleTop, _triangleLeft, _triangleRight, _hue, triangleWidth, triangleHeight);
        context.DrawImage(svTriangle, new Rect(0, 0, triangleWidth, triangleHeight));

        if (_selectionPoint.HasValue)
        {
            Point sp = _selectionPoint.Value;
            Brush contrast = new SolidColorBrush(GetContrasColor(SelectedColor));

            context.DrawEllipse(null, new Pen(contrast, 1), sp, 5, 5);
            context.DrawEllipse(new SolidColorBrush(SelectedColor), new Pen(contrast, 1), sp, 3, 3);
        }
    }

    /// <summary>
    /// Returns a contrasting color (black or white) for the given color based on luminance.
    /// </summary>
    /// <param name="c">The color to test.</param>
    /// <returns>Black or white color for contrast.</returns>
    public static Color GetContrasColor(Color c)
    {
        double luminance = (0.299 * c.R + 0.587 * c.G + 0.114 * c.B) / 255.0;
        return luminance > 0.5 ? Colors.Black : Colors.White;
    }

    private static void ColorToHSV(Color color, out double h, out double s, out double v)
    {
        double r = color.R / 255.0;
        double g = color.G / 255.0;
        double b = color.B / 255.0;

        double max = Math.Max(r, Math.Max(g, b));
        double min = Math.Min(r, Math.Min(g, b));
        double delta = max - min;

        h = 0;

        if (delta != 0)
        {
            if (max == r)
                h = 60 * (((g - b) / delta) % 6);
            else if (max == g)
                h = 60 * (((b - r) / delta) + 2);
            else if (max == b)
                h = 60 * (((r - g) / delta) + 4);
        }

        if (h < 0)
            h += 360;

        s = max == 0 ? 0 : delta / max;
        v = max;
    }

    /// <summary>
    /// Converts a color from HSV (Hue, Saturation, Value) color space to an RGB <see cref="Color"/>.
    /// </summary>
    /// <param name="h">
    /// The hue component, in degrees, where 0 ≤ h &lt; 360. Represents the color type.
    /// </param>
    /// <param name="s">
    /// The saturation component, in the range [0, 1]. Represents the vibrancy of the color, where 0 is grayscale and 1 is the full color.
    /// </param>
    /// <param name="v">
    /// The value (brightness) component, in the range [0, 1]. Represents the brightness of the color, where 0 is black and 1 is the brightest.
    /// </param>
    /// <returns>
    /// A <see cref="Color"/> structure representing the equivalent color in the RGB color space.
    /// </returns>
    /// <remarks>
    /// This method calculates the RGB representation of a color based on its HSV components.
    /// The conversion algorithm divides the hue into six sectors and computes the RGB values accordingly.
    /// The resulting color is returned as an RGB <see cref="Color"/> with 8-bit channels.
    /// </remarks>
    public static Color ColorFromHSV(double h, double s, double v)
    {
        double c = v * s;
        double x = c * (1 - Math.Abs((h / 60) % 2 - 1));
        double m = v - c;

        double r = 0, g = 0, b = 0;
        int sector = (int)(h / 60) % 6;

        switch (sector)
        {
            case 0: r = c; g = x; b = 0; break;
            case 1: r = x; g = c; b = 0; break;
            case 2: r = 0; g = c; b = x; break;
            case 3: r = 0; g = x; b = c; break;
            case 4: r = x; g = 0; b = c; break;
            case 5: r = c; g = 0; b = x; break;
        }

        return Color.FromRgb(
            (byte)((r + m) * 255),
            (byte)((g + m) * 255),
            (byte)((b + m) * 255)
        );
    }

    private WriteableBitmap RenderSaturationValueTriangle(Point top, Point left, Point right, double hue, int width, int height)
    {
        WriteableBitmap bmp = new(new PixelSize(width, height), new Vector(96, 96), PixelFormat.Bgra8888);

        using (ILockedFramebuffer fb = bmp.Lock())
        {
            byte[] buffer = new byte[fb.RowBytes * height];
            int stride = fb.RowBytes / 4;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Point p = new(x, y);

                    if (IsPointInTriangle(p, top, left, right, out double u, out double v, out double w))
                    {
                        double s = u;
                        double vVal = u + w;

                        Color color = ColorFromHSV(hue, s, vVal);
                        int i = y * fb.RowBytes + x * 4;

                        buffer[i + 0] = color.B;
                        buffer[i + 1] = color.G;
                        buffer[i + 2] = color.R;
                        buffer[i + 3] = color.A;
                    }
                }
            }

            System.Runtime.InteropServices.Marshal.Copy(buffer, 0, fb.Address, buffer.Length);
        }

        return bmp;
    }

    private static bool IsPointInTriangle(Point p, Point a, Point b, Point c, out double u, out double v, out double w)
    {
        double denom = ((b.Y - c.Y) * (a.X - c.X) + (c.X - b.X) * (a.Y - c.Y));
        u = ((b.Y - c.Y) * (p.X - c.X) + (c.X - b.X) * (p.Y - c.Y)) / denom;
        v = ((c.Y - a.Y) * (p.X - c.X) + (a.X - c.X) * (p.Y - c.Y)) / denom;
        w = 1 - u - v;
        return u >= 0 && v >= 0 && w >= 0;
    }

    private bool IsHueRing(Point pos, Point center, double innerRadius, double outerRadius)
    {
        double dx = pos.X - center.X;
        double dy = pos.Y - center.Y;
        double distSq = dx * dx + dy * dy;
        return distSq >= innerRadius * innerRadius && distSq <= outerRadius * outerRadius;
    }
}
