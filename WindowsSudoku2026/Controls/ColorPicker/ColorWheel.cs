using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WindowsSudoku2026.Common.Utils;

namespace WindowsSudoku2026.Controls;

public class ColorWheel : Control
{
    private double _radius;
    private Point _center;
    private Point _marker;

    private readonly DrawingVisual _markerVisual = new();
    private readonly DrawingVisual _bitmapVisual = new();

    private const double MarkerRadius = 6;
    private const double MarkerStroke = 2;
    private const double MarkerPadding = MarkerRadius + MarkerStroke;

    private WriteableBitmap? _bitmap;

    public Hsv HsvData
    {
        get { return (Hsv)GetValue(HsvDataProperty); }
        set { SetValue(HsvDataProperty, value); }
    }

    // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty HsvDataProperty =
        DependencyProperty.Register(nameof(HsvData), typeof(Hsv), typeof(ColorWheel), new PropertyMetadata(new Hsv(1, 1, 1)));

    public Color SelectedColor
    {
        get { return (Color)GetValue(SelectedColorProperty); }
        set { SetValue(SelectedColorProperty, value); }
    }

    // Using a DependencyProperty as the backing store for SelectedColor.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty SelectedColorProperty =
        DependencyProperty.Register(nameof(SelectedColor), typeof(Color), typeof(ColorWheel), new PropertyMetadata(Colors.White, OnSelectedColorChanged));

    private static void OnSelectedColorChanged(
    DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var wheel = (ColorWheel)d;
        wheel.UpdateMarkerFromColor((Color)e.NewValue);

        // --- TODO: RGB → HSV ---
        var color = (Color)e.NewValue;

        double r = color.R / 255.0;
        double g = color.G / 255.0;
        double b = color.B / 255.0;

        double max = Math.Max(r, Math.Max(g, b));
        double min = Math.Min(r, Math.Min(g, b));
        double delta = max - min;

        double hue = 0;
        if (delta != 0)
        {
            if (max == r) hue = 60 * (((g - b) / delta) % 6);
            else if (max == g) hue = 60 * (((b - r) / delta) + 2);
            else if (max == b) hue = 60 * (((r - g) / delta) + 4);
        }
        if (hue < 0) hue += 360;

        double sat = max == 0 ? 0 : delta / max;
        double val = max; // V entspricht max(R,G,B)

        wheel.HsvData = new Hsv(hue, sat, val);
    }

    static ColorWheel()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ColorWheel), new FrameworkPropertyMetadata(typeof(ColorWheel)));
    }
    public ColorWheel()
    {
        Focusable = true;
        ClipToBounds = false;
        UseLayoutRounding = true;
        SnapsToDevicePixels = true;

        AddVisualChild(_markerVisual);
        AddLogicalChild(_markerVisual);
        AddVisualChild(_bitmapVisual);
        AddLogicalChild(_bitmapVisual);
    }
    protected override int VisualChildrenCount => 2;

    protected override Visual GetVisualChild(int index)
    {
        return index switch
        {
            0 => _bitmapVisual,
            _ => _markerVisual
        };
    }

    protected override Size MeasureOverride(Size available)
    {
        double size = Math.Min(available.Width, available.Height);
        return new Size(size, size);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        double size = Math.Min(finalSize.Width, finalSize.Height);

        _radius = (size / 2) - MarkerPadding;
        _center = new Point(size / 2, size / 2);

        if (_marker == default)
            _marker = _center;

        DrawBitmapWheel();
        DrawMarker();

        return new Size(size, size);
    }
    private void DrawBitmapWheel()
    {
        int size = (int)(_radius * 2);

        _bitmap = new WriteableBitmap(
            size, size, 96, 96, PixelFormats.Bgra32, null);

        int stride = size * 4;
        byte[] pixels = new byte[size * stride];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                double dx = x - size / 2.0;
                double dy = y - size / 2.0;
                double dist = Math.Sqrt(dx * dx + dy * dy);

                if (dist > _radius)
                    continue;

                double angle = Math.Atan2(dy, dx);
                if (angle < 0) angle += 2 * Math.PI;

                double hue = angle * 180 / Math.PI;
                double sat = dist / _radius;

                Color c = HsvToRgb(hue, sat, 1);

                int i = y * stride + x * 4;
                pixels[i + 0] = c.B;
                pixels[i + 1] = c.G;
                pixels[i + 2] = c.R;
                pixels[i + 3] = 255;
            }
        }

        _bitmap.WritePixels(
            new Int32Rect(0, 0, size, size),
            pixels, stride, 0);

        using var dc = _bitmapVisual.RenderOpen();
        dc.DrawImage(_bitmap,
            new Rect(
                _center.X - _radius,
                _center.Y - _radius,
                size, size));
    }

    private void DrawMarker()
    {
        using var dc = _markerVisual.RenderOpen();

        dc.DrawEllipse(
            null,
            new Pen(Brushes.DarkViolet, 2),
            _marker,
            6, 6);
    }

    private static Color HsvToRgb(double h, double s, double v)
    {
        int hi = (int)(h / 60) % 6;
        double f = h / 60 - Math.Floor(h / 60);

        v *= 255;
        byte p = (byte)(v * (1 - s));
        byte q = (byte)(v * (1 - f * s));
        byte t = (byte)(v * (1 - (1 - f) * s));
        byte vb = (byte)v;

        return hi switch
        {
            0 => Color.FromRgb(vb, t, p),
            1 => Color.FromRgb(q, vb, p),
            2 => Color.FromRgb(p, vb, t),
            3 => Color.FromRgb(p, q, vb),
            4 => Color.FromRgb(t, p, vb),
            _ => Color.FromRgb(vb, p, q)
        };
    }
    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        CaptureMouse();
        UpdateFromBitmapMouse(e.GetPosition(this));
        e.Handled = true;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (IsMouseCaptured)
            UpdateFromBitmapMouse(e.GetPosition(this));
    }

    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
        ReleaseMouseCapture();
        e.Handled = true;
    }
    private void UpdateMarkerFromColor(Color color)
    {
        // Alpha ignorieren für Position
        double r = color.R / 255.0;
        double g = color.G / 255.0;
        double b = color.B / 255.0;

        // RGB -> HSV
        double max = Math.Max(r, Math.Max(g, b));
        double min = Math.Min(r, Math.Min(g, b));
        double delta = max - min;

        double hue = 0;
        if (delta != 0)
        {
            if (max == r) hue = 60 * (((g - b) / delta) % 6);
            else if (max == g) hue = 60 * (((b - r) / delta) + 2);
            else if (max == b) hue = 60 * (((r - g) / delta) + 4);
        }
        if (hue < 0) hue += 360;

        double sat = max == 0 ? 0 : delta / max;

        // Markerposition berechnen
        double angle = hue * Math.PI / 180;
        double radius = sat * _radius;

        Vector v = new Vector(Math.Cos(angle), Math.Sin(angle)) * radius;
        _marker = _center + v;

        DrawMarker();
    }
    private void UpdateFromBitmapMouse(Point p)
    {
        Vector v = p - _center;
        double dist = v.Length;

        if (dist == 0)
            return;

        // 👉 Clamp wie im alten Code
        double clampedDist = Math.Min(dist, _radius);
        Vector clamped = v * (clampedDist / dist);

        // 👉 HSV exakt berechnen (NICHT aus Bitmap!)
        double angle = Math.Atan2(clamped.Y, clamped.X);
        if (angle < 0) angle += 2 * Math.PI;

        double hue = angle * 180 / Math.PI;
        double sat = clampedDist / _radius;

        Color color = HsvToRgb(hue, sat, 1);

        // 👉 Marker korrekt positionieren
        _marker = _center + clamped;
        DrawMarker();

        // HSV aktualisieren
        HsvData = new Hsv(hue, sat, 1);

        SelectedColor = color;
    }
}
