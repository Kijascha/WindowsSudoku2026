using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WindowsSudoku2026.Controls;


public class PreviewColor : Control
{
    private readonly DrawingVisual _visual = new();

    public static readonly DependencyProperty PreviewSelectedColorProperty =
        DependencyProperty.Register(nameof(PreviewSelectedColor), typeof(Color), typeof(PreviewColor),
            new PropertyMetadata(Colors.White, OnPreviewColorChanged));

    public Color PreviewSelectedColor
    {
        get => (Color)GetValue(PreviewSelectedColorProperty);
        set => SetValue(PreviewSelectedColorProperty, value);
    }

    static PreviewColor()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(PreviewColor),
            new FrameworkPropertyMetadata(typeof(PreviewColor)));
    }

    public PreviewColor()
    {
        AddVisualChild(_visual);
        AddLogicalChild(_visual);
        Focusable = false;
        SizeChanged += (s, e) => Draw();
    }

    protected override int VisualChildrenCount => 1;
    protected override Visual GetVisualChild(int index) => _visual;

    protected override Size MeasureOverride(Size available)
    {
        double size = double.IsInfinity(available.Width) ? 50 : available.Width;
        double height = double.IsInfinity(available.Height) ? 50 : available.Height;
        double square = Math.Min(size, height);
        return new Size(square, square);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        double square = Math.Min(finalSize.Width, finalSize.Height);
        Draw();
        return new Size(square, square);
    }

    private static void OnPreviewColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((PreviewColor)d).Draw();
    }

    private void Draw()
    {
        double width = ActualWidth;
        double height = ActualHeight;
        if (width <= 0 || height <= 0) return;

        using var dc = _visual.RenderOpen();

        // Farbe zeichnen
        var brush = new SolidColorBrush(PreviewSelectedColor);
        dc.DrawRectangle(brush, new Pen(Brushes.Black, 1), new Rect(0, 0, width, height));
    }
}
