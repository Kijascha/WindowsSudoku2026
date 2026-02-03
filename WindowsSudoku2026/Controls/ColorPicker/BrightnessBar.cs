using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WindowsSudoku2026.Common.Utils;

namespace WindowsSudoku2026.Controls;

public class BrightnessBar : Control
{
    private readonly DrawingVisual _visual = new();
    private double _markerY;

    // H/S aus ColorWheel
    public static readonly DependencyProperty HsvProperty =
        DependencyProperty.Register(nameof(Hsv), typeof(Hsv), typeof(BrightnessBar),
            new PropertyMetadata(new Hsv(0, 0, 1), OnHsvChanged));

    // Value 0..1
    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(double), typeof(BrightnessBar),
            new PropertyMetadata(1.0, OnValueChanged));

    public Hsv Hsv
    {
        get => (Hsv)GetValue(HsvProperty);
        set => SetValue(HsvProperty, value);
    }

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public Color FinalColor
    {
        get { return (Color)GetValue(FinalColorProperty); }
        set { SetValue(FinalColorProperty, value); }
    }

    // Using a DependencyProperty as the backing store for FinalColor.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty FinalColorProperty =
        DependencyProperty.Register(nameof(FinalColor), typeof(Color), typeof(BrightnessBar), new PropertyMetadata(Colors.White));


    public event EventHandler<double>? ValueChanged;

    static BrightnessBar()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(BrightnessBar), new FrameworkPropertyMetadata(typeof(BrightnessBar)));
    }

    public BrightnessBar()
    {
        AddVisualChild(_visual);
        AddLogicalChild(_visual);
        Focusable = true;

        SizeChanged += (s, e) => Draw(); // initial zeichnen
    }

    protected override int VisualChildrenCount => 1;
    protected override Visual GetVisualChild(int index) => _visual;

    protected override Size MeasureOverride(Size available)
    {
        double width = 30;
        double height = double.IsInfinity(available.Height) ? 200 : available.Height;
        return new Size(width, height);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        Draw();
        return finalSize;
    }

    private static void OnHsvChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((BrightnessBar)d).Draw();
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var bar = (BrightnessBar)d;
        bar.Draw();
        bar.ValueChanged?.Invoke(bar, bar.Value);

    }

    private void Draw()
    {
        double width = ActualWidth;
        double height = ActualHeight;
        if (width <= 0 || height <= 0) return;

        // Gradient von H/S V=1 bis V=0
        var brush = new LinearGradientBrush
        {
            StartPoint = new Point(0.5, 0),
            EndPoint = new Point(0.5, 1),
        };

        // Top: aktuelle H/S, V=1
        brush.GradientStops.Add(new GradientStop(HsvToRgb(Hsv.H, Hsv.S, 1), 0));
        // Bottom: schwarz, V=0
        brush.GradientStops.Add(new GradientStop(Colors.Black, 1));

        using var dc = _visual.RenderOpen();
        dc.DrawRectangle(brush, null, new Rect(0, 0, width, height));

        FinalColor = HsvToRgb(Hsv.H, Hsv.S, Value);

        // Marker
        _markerY = (1 - Value) * height;
        dc.DrawLine(new Pen(Brushes.White, 2), new Point(0, _markerY), new Point(width, _markerY));
    }

    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        CaptureMouse();
        UpdateValueFromPoint(e.GetPosition(this));
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (IsMouseCaptured)
            UpdateValueFromPoint(e.GetPosition(this));
    }

    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
        ReleaseMouseCapture();
    }

    private void UpdateValueFromPoint(Point p)
    {
        double y = Math.Max(0, Math.Min(ActualHeight, p.Y));
        Value = 1 - (y / ActualHeight); // Value setzen, Event feuern
    }

    // HSV → RGB Utility
    private static Color HsvToRgb(double h, double s, double v)
    {
        int hi = (int)(h / 60) % 6;
        double f = h / 60 - Math.Floor(h / 60);
        v *= 255;
        byte vb = (byte)v;
        byte p = (byte)(v * (1 - s));
        byte q = (byte)(v * (1 - f * s));
        byte t = (byte)(v * (1 - (1 - f) * s));

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
}
