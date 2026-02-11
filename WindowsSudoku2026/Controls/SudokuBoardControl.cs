using CommunityToolkit.Mvvm.Messaging;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WindowsSudoku2026.Common.Enums;
using WindowsSudoku2026.Common.Models;
using WindowsSudoku2026.Common.Utils.Colors;
using WindowsSudoku2026.Core.Helpers;
using WindowsSudoku2026.Core.Messaging;

namespace WindowsSudoku2026.Controls;

public class SudokuBoardControl : Control
{
    private double _cellSize;
    private bool _isMouseDown = false;
    private (int row, int col)? _selectedCell;
    private HashSet<(int row, int col)> _selectedCells = new();

    private static readonly Point[] CornerMarkPositions =
    {
        new(0.18, 0.18), // TopLeft
        new(0.82, 0.18), // TopRight
        new(0.18, 0.82), // BottomLeft
        new(0.82, 0.82), // BottomRight
        new(0.50, 0.18), // Top
        new(0.50, 0.82), // Bottom
        new(0.18, 0.50), // Left
        new(0.82, 0.50), // Right
        new(0.40, 0.42), // Center (leicht Richtung TopLeft)
    };
    private static readonly Point[] SolverMarkPositions =
    {
        new(0.18, 0.18), // TopLeft
        new(0.82, 0.18), // TopRight
        new(0.18, 0.82), // BottomLeft
        new(0.82, 0.82), // BottomRight
        new(0.50, 0.18), // Top
        new(0.50, 0.82), // Bottom
        new(0.18, 0.50), // Left
        new(0.82, 0.50), // Right
        new(0.50, 0.50), // Center
    };

    #region Dependency Properties


    public bool IsSolverMode
    {
        get { return (bool)GetValue(IsSolverModeProperty); }
        set { SetValue(IsSolverModeProperty, value); }
    }

    // Using a DependencyProperty as the backing store for IsSolverMode.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty IsSolverModeProperty =
        DependencyProperty.Register(nameof(IsSolverMode), typeof(bool), typeof(SudokuBoardControl), new PropertyMetadata(false));


    public bool IsLocked
    {
        get { return (bool)GetValue(IsLockedProperty); }
        set { SetValue(IsLockedProperty, value); }
    }

    // Using a DependencyProperty as the backing store for IsLocked.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty IsLockedProperty =
        DependencyProperty.Register(nameof(IsLocked), typeof(bool), typeof(SudokuBoardControl), new PropertyMetadata(false));

    public GameType GameMode
    {
        get { return (GameType)GetValue(GameModeProperty); }
        set { SetValue(GameModeProperty, value); }
    }

    // Using a DependencyProperty as the backing store for GameMode.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty GameModeProperty =
        DependencyProperty.Register(nameof(GameMode), typeof(GameType), typeof(SudokuBoardControl), new PropertyMetadata(GameType.Play, OnGameTypeChanged));

    private static void OnGameTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {

        var control = (SudokuBoardControl)d;

        if (control.GameMode == GameType.Create && control.IsSolverMode)
            control.DrawSolverMarks();
    }

    public bool IsFilled
    {
        get { return (bool)GetValue(IsFilledProperty); }
        set { SetValue(IsFilledProperty, value); }
    }

    // Using a DependencyProperty as the backing store for IsFilled.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty IsFilledProperty =
        DependencyProperty.Register(nameof(IsFilled), typeof(bool), typeof(SudokuBoardControl), new PropertyMetadata(false));


    public ModifierKeys ActiveModifiers
    {
        get => (ModifierKeys)GetValue(ActiveModifiersProperty);
        set => SetValue(ActiveModifiersProperty, value);
    }

    public static readonly DependencyProperty ActiveModifiersProperty =
        DependencyProperty.Register(
            nameof(ActiveModifiers),
            typeof(ModifierKeys),
            typeof(SudokuBoardControl),
            new FrameworkPropertyMetadata(ModifierKeys.None));

    public Pen ThinGridPen
    {
        get { return (Pen)GetValue(ThinGridPenProperty); }
        set { SetValue(ThinGridPenProperty, value); }
    }
    // Using a DependencyProperty as the backing store for ThinGridPen.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty ThinGridPenProperty =
        DependencyProperty.Register(nameof(ThinGridPen), typeof(Pen), typeof(SudokuBoardControl), new PropertyMetadata(new Pen(Brushes.Black, 1), OnVisualPropertyChanged));

    public Pen ThickGridPen
    {
        get { return (Pen)GetValue(ThickGridPenProperty); }
        set { SetValue(ThickGridPenProperty, value); }
    }

    // Using a DependencyProperty as the backing store for ThickGridPenn.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty ThickGridPenProperty =
        DependencyProperty.Register(nameof(ThickGridPen), typeof(Pen), typeof(SudokuBoardControl), new PropertyMetadata(new Pen(Brushes.Black, 2), OnVisualPropertyChanged));

    public Brush CellBackground
    {
        get { return (Brush)GetValue(CellBackgroundProperty); }
        set { SetValue(CellBackgroundProperty, value); }
    }

    // Using a DependencyProperty as the backing store for CellBackground.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty CellBackgroundProperty =
        DependencyProperty.Register(nameof(CellBackground), typeof(Brush), typeof(SudokuBoardControl), new PropertyMetadata(Brushes.White, OnVisualPropertyChanged));

    public Pen OuterBorderPen
    {
        get => (Pen)GetValue(OuterBorderPenProperty);
        set => SetValue(OuterBorderPenProperty, value);
    }

    public static readonly DependencyProperty OuterBorderPenProperty =
        DependencyProperty.Register(
            nameof(OuterBorderPen),
            typeof(Pen),
            typeof(SudokuBoardControl),
            new PropertyMetadata(new Pen(Brushes.Black, 3)));

    public Brush SelectionBrush
    {
        get => (Brush)GetValue(SelectionBrushProperty);
        set => SetValue(SelectionBrushProperty, value);
    }

    public static readonly DependencyProperty SelectionBrushProperty =
        DependencyProperty.Register(
            nameof(SelectionBrush),
            typeof(Brush),
            typeof(SudokuBoardControl),
            new PropertyMetadata(
                Brushes.DeepSkyBlue,
                OnVisualPropertyChanged));
    public Brush CenterMarkBrush
    {
        get => (Brush)GetValue(CenterMarkBrushProperty);
        set => SetValue(CenterMarkBrushProperty, value);
    }

    public static readonly DependencyProperty CenterMarkBrushProperty =
        DependencyProperty.Register(
            nameof(CenterMarkBrush),
            typeof(Brush),
            typeof(SudokuBoardControl),
            new PropertyMetadata(Brushes.Blue, OnVisualPropertyChanged));

    private static void OnVisualPropertyChanged(
    DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SudokuBoardControl control)
        {
            control.InvalidateVisuals();
        }
    }
    private void InvalidateVisuals()
    {
        if (_cellSize <= 0)
            return;

        ArrangeVisuals(ActualWidth);
    }



    public InputActionType InputActionType
    {
        get { return (InputActionType)GetValue(InputActionTypeProperty); }
        set { SetValue(InputActionTypeProperty, value); }
    }

    // Using a DependencyProperty as the backing store for InputActionType.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty InputActionTypeProperty =
        DependencyProperty.Register(nameof(InputActionType), typeof(InputActionType), typeof(SudokuBoardControl), new PropertyMetadata(InputActionType.Digits));



    public static readonly DependencyProperty DigitInputAsIntCommandProperty =
    DependencyProperty.Register(nameof(DigitInputAsIntCommand), typeof(ICommand), typeof(SudokuBoardControl));

    public ICommand DigitInputAsIntCommand
    {
        get => (ICommand)GetValue(DigitInputAsIntCommandProperty);
        set => SetValue(DigitInputAsIntCommandProperty, value);
    }

    public IPuzzle Puzzle
    {
        get => (IPuzzle)GetValue(PuzzleProperty);
        set => SetValue(PuzzleProperty, value);
    }

    public static readonly DependencyProperty PuzzleProperty =
        DependencyProperty.Register(
            nameof(Puzzle),
            typeof(IPuzzle),
            typeof(SudokuBoardControl),
            new FrameworkPropertyMetadata(
                new Puzzle(), OnPuzzleChanged));

    private static void OnPuzzleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (SudokuBoardControl)d;

        if (e.OldValue is INotifyPropertyChanged oldPuzzle)
            oldPuzzle.PropertyChanged -= control.OnPuzzlePropertyChanged;

        if (e.NewValue is INotifyPropertyChanged newPuzzle)
            newPuzzle.PropertyChanged += control.OnPuzzlePropertyChanged;

        control.InvalidateVisuals();
        control.InvalidateVisual();
        control.RedrawAll();


        if (d is SudokuBoardControl control2)
        {
            // Wir gehen sicher, dass wir auf dem UI-Thread sind
            control2.Dispatcher.InvokeAsync(() =>
            {
                control2.RedrawAll();
                control2.InvalidateVisual();
            }, System.Windows.Threading.DispatcherPriority.Render);
        }
    }

    private void OnPuzzlePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        RedrawAll();
    }

    private void RedrawAll()
    {

        // Falls das Puzzle null ist (wegen dem Reset im ViewModel), 
        // müssen wir die Visuals leeren, statt sie zu durchlaufen.
        if (Puzzle == null)
        {
            _digitsVisual.RenderOpen().Close(); // Leert den Layer
            _cellColorsVisual.RenderOpen().Close();
            _centerMarksVisual.RenderOpen().Close();
            _cornerMarksVisual.RenderOpen().Close();
            _solverMarksVisual.RenderOpen().Close();
            // ... andere Layer ebenfalls "leeren"
            return;
        }

        DrawCellColors();
        DrawCenterMarks();
        DrawCornerMarks();
        if (GameMode == GameType.Create && IsSolverMode)
            DrawSolverMarks();
        if (GameMode == GameType.Play)
            DrawConflictHighlights();
        DrawDigits();
    }
    #endregion

    #region Visual Layers
    private readonly DrawingVisual _gridVisual = new DrawingVisual();
    private readonly DrawingVisual _cellsVisual = new DrawingVisual();
    private readonly DrawingVisual _cellColorsVisual = new DrawingVisual();
    private readonly DrawingVisual _selectionVisual = new DrawingVisual();
    private readonly DrawingVisual _centerMarksVisual = new DrawingVisual();
    private readonly DrawingVisual _cornerMarksVisual = new DrawingVisual();
    private readonly DrawingVisual _solverMarksVisual = new DrawingVisual();
    private readonly DrawingVisual _digitsVisual = new DrawingVisual();
    private readonly DrawingVisual _conflictHighlightsVisual = new DrawingVisual();

    private void AddLayer(DrawingVisual visual, bool crisp = true)
    {
        RenderOptions.SetEdgeMode(
            visual,
            crisp ? EdgeMode.Aliased : EdgeMode.Unspecified);

        RenderOptions.SetBitmapScalingMode(
            visual,
            crisp ? BitmapScalingMode.NearestNeighbor : BitmapScalingMode.HighQuality);

        AddVisualChild(visual);
        AddLogicalChild(visual);
    }
    protected override int VisualChildrenCount => 9;

    protected override Visual GetVisualChild(int index)
    {
        return index switch
        {
            0 => _cellsVisual,
            1 => _cellColorsVisual,
            2 => _selectionVisual,
            3 => _centerMarksVisual,
            4 => _cornerMarksVisual,
            5 => _solverMarksVisual,
            6 => _digitsVisual,
            7 => _conflictHighlightsVisual,
            8 => _gridVisual,
            _ => throw new ArgumentOutOfRangeException(nameof(index))
        };
    }
    #endregion

    #region Constructor
    static SudokuBoardControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(SudokuBoardControl),
            new FrameworkPropertyMetadata(typeof(SudokuBoardControl)));
    }
    public SudokuBoardControl()
    {
        Focusable = true;
        ClipToBounds = true;
        UseLayoutRounding = true;
        SnapsToDevicePixels = true;
        //RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);
        RenderOptions.SetClearTypeHint(this, ClearTypeHint.Enabled);

        AddLayer(_cellsVisual);
        AddLayer(_cellColorsVisual, false);
        AddLayer(_selectionVisual);
        AddLayer(_centerMarksVisual);
        AddLayer(_cornerMarksVisual);
        AddLayer(_solverMarksVisual);
        AddLayer(_digitsVisual);
        AddLayer(_conflictHighlightsVisual);
        AddLayer(_gridVisual);


        this.Loaded += (s, e) =>
        {
            WeakReferenceMessenger.Default.Register<RequestCaptureMessage>(this, (r, m) =>
            {
                // Wir prüfen SOFORT, ob schon jemand geantwortet hat
                if (m.HasReceivedResponse) return;

                // UI-Thread sicherstellen
                this.Dispatcher.Invoke(() =>
                {
                    // Sicherheitscheck: Wurde diese spezifische Nachricht schon beantwortet?
                    if (m.HasReceivedResponse) return;

                    // Sicherstellen, dass das UI aktuell ist
                    this.UpdateLayout();

                    // Dein Capturing-Code (RenderTargetBitmap)
                    var preview = this.GeneratePreview();

                    if (preview != null)
                        m.Reply(preview); // Bild zurückschicken
                });
            });
        };

        this.Unloaded += (s, e) =>
        {
            WeakReferenceMessenger.Default.Unregister<RequestCaptureMessage>(this);
        };
    }
    #endregion

    #region Layout Overrides
    protected override Size MeasureOverride(Size availableSize)
    {
        double size = Math.Min(availableSize.Width, availableSize.Height);
        int cell = (int)(size / 9);
        int board = cell * 9;
        return new Size(board, board);
    }
    protected override Size ArrangeOverride(Size finalSize)
    {
        ArrangeVisuals(finalSize.Width);
        return finalSize;
    }
    private void ArrangeVisuals(double finalSize)
    {
        int boardPixels = (int)Math.Floor(finalSize);
        int cellPixels = boardPixels / IPuzzle.Size;
        boardPixels = cellPixels * IPuzzle.Size;

        _cellSize = cellPixels;

        DrawCells();
        DrawCellColors();
        DrawSelection(); // hier nötig für den Fall, dass _cellSize sich ändert
        DrawCenterMarks();
        DrawCornerMarks();
        DrawDigits();
        DrawGrid(boardPixels);
    }
    #endregion
    private readonly int[,] _cornerCandidates = new int[9, 9];

    #region Draw Methods
    private void DrawOuterBorder(DrawingContext dc, double boardSize)
    {
        var pen = OuterBorderPen;

        double half = pen.Thickness / 2.0;

        Rect rect = new Rect(
            half + 0.5,
            half + 0.5,
            boardSize - pen.Thickness,
            boardSize - pen.Thickness);

        dc.DrawRectangle(null, pen, rect);
    }
    private void DrawGrid(double boardSize)
    {

        using var dc = _gridVisual.RenderOpen();

        double cell = _cellSize;

        for (int i = 1; i <= 8; i++)
        {
            Pen pen = (i % 3 == 0) ? ThickGridPen : ThinGridPen;
            double pos = i * cell + 0.5;

            dc.DrawLine(pen, new Point(pos, 0), new Point(pos, boardSize));
            dc.DrawLine(pen, new Point(0, pos), new Point(boardSize, pos));
        }

        DrawOuterBorder(dc, boardSize);
    }
    private void DrawCells()
    {
        using var dc = _cellsVisual.RenderOpen();

        for (int r = 0; r < 9; r++)
        {
            for (int c = 0; c < 9; c++)
            {
                double x = c * _cellSize;
                double y = r * _cellSize;
                var rect = new Rect(x, y, _cellSize, _cellSize);

                dc.DrawRectangle(CellBackground, null, rect);
            }
        }
    }
    private void DrawSelection()
    {
        using var dc = _selectionVisual.RenderOpen();

        if (_selectedCells.Count == 0)
            return;

        // --- Hintergrund leicht transparent ---
        foreach (var (r, c) in _selectedCells)
        {
            double x = c * _cellSize;
            double y = r * _cellSize;

            // Hintergrund-Brush: gleiche Farbe wie SelectionBrush, nur Alpha reduzieren
            if (SelectionBrush is SolidColorBrush solid)
            {
                Color bgColor = solid.Color;
                Color transparentColor = Color.FromArgb(20, bgColor.R, bgColor.G, bgColor.B); // sehr leicht
                dc.DrawRectangle(new SolidColorBrush(transparentColor), null, new Rect(x, y, _cellSize, _cellSize));
            }
            else
            {
                // Fallback, falls SelectionBrush kein SolidColorBrush ist
                dc.DrawRectangle(SelectionBrush, null, new Rect(x, y, _cellSize, _cellSize));
            }
        }

        DrawSelectionBorder(dc, _selectedCells, new Pen(SelectionBrush, 6));
    }
    private void DrawSelectionBorder(DrawingContext dc, HashSet<(int row, int col)> cells, Pen pen)
    {
        pen.LineJoin = PenLineJoin.Miter;
        pen.StartLineCap = PenLineCap.Square;
        pen.EndLineCap = PenLineCap.Square;

        double thickness = pen.Thickness;
        double outerInset = OuterBorderPen?.Thickness ?? 0; // Dicke des äußeren Rahmens

        foreach (var (r, c) in cells)
        {
            double x = c * _cellSize;
            double y = r * _cellSize;
            double s = _cellSize;

            // Standard-Inset für alle Linien
            double inset = thickness / 2.0;

            // Anpassung nur für Linien, die am äußeren Rahmen liegen
            double insetTop = inset + (r == 0 ? outerInset : 0);
            double insetLeft = inset + (c == 0 ? outerInset : 0);
            double insetBottom = inset + (r == 8 ? outerInset : 0);
            double insetRight = inset + (c == 8 ? outerInset : 0);

            // Oben
            if (!cells.Contains((r - 1, c)))
                dc.DrawLine(pen, new Point(x + insetLeft, y + insetTop), new Point(x + s - insetRight, y + insetTop));

            // Rechts
            if (!cells.Contains((r, c + 1)))
                dc.DrawLine(pen, new Point(x + s - insetRight, y + insetTop), new Point(x + s - insetRight, y + s - insetBottom));

            // Unten
            if (!cells.Contains((r + 1, c)))
                dc.DrawLine(pen, new Point(x + s - insetRight, y + s - insetBottom), new Point(x + insetLeft, y + s - insetBottom));

            // Links
            if (!cells.Contains((r, c - 1)))
                dc.DrawLine(pen, new Point(x + insetLeft, y + s - insetBottom), new Point(x + insetLeft, y + insetTop));

            // --- Ecken Logik ---
            double cornerSize = thickness;

            // Obere linke Ecke
            if (cells.Contains((r - 1, c)) && cells.Contains((r, c - 1)) && !cells.Contains((r - 1, c - 1)))
                dc.DrawRectangle(pen.Brush, null, new Rect(x + insetLeft - cornerSize / 2, y + insetTop - cornerSize / 2, cornerSize, cornerSize));

            // Obere rechte Ecke
            if (cells.Contains((r - 1, c)) && cells.Contains((r, c + 1)) && !cells.Contains((r - 1, c + 1)))
                dc.DrawRectangle(pen.Brush, null, new Rect(x + s - insetRight - cornerSize / 2, y + insetTop - cornerSize / 2, cornerSize, cornerSize));

            // Untere rechte Ecke
            if (cells.Contains((r + 1, c)) && cells.Contains((r, c + 1)) && !cells.Contains((r + 1, c + 1)))
                dc.DrawRectangle(pen.Brush, null, new Rect(x + s - insetRight - cornerSize / 2, y + s - insetBottom - cornerSize / 2, cornerSize, cornerSize));

            // Untere linke Ecke
            if (cells.Contains((r + 1, c)) && cells.Contains((r, c - 1)) && !cells.Contains((r + 1, c - 1)))
                dc.DrawRectangle(pen.Brush, null, new Rect(x + insetLeft - cornerSize / 2, y + s - insetBottom - cornerSize / 2, cornerSize, cornerSize));
        }
    }
    private void DrawCellColors()
    {
        using var dc = _cellColorsVisual.RenderOpen();

        if (Puzzle == null) return;

        for (int r = 0; r < 9; r++)
        {
            for (int c = 0; c < 9; c++)
            {
                var colors = Puzzle[r, c].CellColors;
                if (colors.Count == 0)
                    continue;

                double x = c * _cellSize;
                double y = r * _cellSize;
                Rect rect = new Rect(x, y, _cellSize, _cellSize);

                if (colors.Count == 1)
                {
                    dc.DrawRectangle(
                        ColorPaletteFactory.GetBrush(Puzzle.ActivePalette, colors[0]),
                        null,
                        rect);
                }
                else
                {
                    DrawMultiColorCell(dc, rect, colors.ToList());
                }
            }
        }
    }
    private void DrawMultiColorCell(
    DrawingContext dc,
    Rect rect,
    List<SudokuCellColor> colors)
    {
        int count = colors.Count;

        if (count == 1)
        {
            dc.DrawRectangle(
                ColorPaletteFactory.GetBrush(Puzzle.ActivePalette, colors[0]),
                null,
                rect);
            return;
        }

        if (count == 2)
        {
            DrawTwoColorCell(dc, rect, colors);
            return;
        }

        DrawRadialMultiColorCell(dc, rect, colors);
    }
    private void DrawTwoColorCell(
    DrawingContext dc,
    Rect rect,
    List<SudokuCellColor> colors)
    {
        if (colors.Count < 2)
            return;

        double w = rect.Width;
        double h = rect.Height;

        double tilt = h * 0.25;
        double half = tilt / 2;


        // 1️⃣ komplette Zelle links füllen
        dc.DrawRectangle(
            ColorPaletteFactory.GetBrush(Puzzle.ActivePalette, colors[0]),
            null,
            rect);

        // 2️⃣ rechte Fläche als Polygon
        var geo = new StreamGeometry();
        using (var ctx = geo.Open())
        {
            ctx.BeginFigure(
                new Point(rect.X + w / 2 + half, rect.Y),
                true, true);

            ctx.LineTo(new Point(rect.Right, rect.Y), true, false);
            ctx.LineTo(new Point(rect.Right, rect.Bottom), true, false);
            ctx.LineTo(new Point(rect.X + w / 2 - half, rect.Bottom), true, false);
        }
        geo.Freeze();

        dc.DrawGeometry(
            ColorPaletteFactory.GetBrush(Puzzle.ActivePalette, colors[1]),
            null,
            geo);
    }
    private void DrawRadialMultiColorCell(
    DrawingContext dc,
    Rect rect,
    List<SudokuCellColor> colors)
    {
        int count = colors.Count;
        if (count < 3)
            return;

        Point center = new(
            rect.X + rect.Width / 2,
            rect.Y + rect.Height / 2);

        // Radius bewusst größer als Zelle → garantiert vollständige Füllung
        double radius = Math.Sqrt(rect.Width * rect.Width + rect.Height * rect.Height);

        double angleStep = 360.0 / count;

        // ⭐ globaler Tilt (optisch ruhig)
        double tiltAngle = 16; // Grad – hier kannst du feinjustieren
        double startAngle = -90 + tiltAngle;
        dc.PushClip(
    new RectangleGeometry(
        new Rect(
            rect.X - 0.5,
            rect.Y - 0.5,
            rect.Width + 1,
            rect.Height + 1)));

        for (int i = 0; i < count; i++)
        {
            double a0 = (startAngle + i * angleStep) * Math.PI / 180.0;
            double a1 = (startAngle + (i + 1) * angleStep) * Math.PI / 180.0;

            Point p0 = new(
                center.X + Math.Cos(a0) * radius,
                center.Y + Math.Sin(a0) * radius);

            Point p1 = new(
                center.X + Math.Cos(a1) * radius,
                center.Y + Math.Sin(a1) * radius);

            var geo = new StreamGeometry();
            using (var ctx = geo.Open())
            {
                ctx.BeginFigure(center, true, true);
                //ctx.LineTo(p0, true, false);
                //ctx.LineTo(p1, true, false); 
                ctx.LineTo(p0, true, false);
                ctx.ArcTo(
                    p1,
                    new Size(radius, radius),
                    0,
                    false,
                    SweepDirection.Clockwise,
                    true,
                    false);
            }
            geo.Freeze();

            dc.DrawGeometry(
                ColorPaletteFactory.GetBrush(Puzzle.ActivePalette, colors[i]),
                null,
                geo);
        }

        dc.Pop();
    }
    private void DrawCenterMarks()
    {
        using var dc = _centerMarksVisual.RenderOpen();

        if (Puzzle == null) return;

        for (int r = 0; r < IPuzzle.Size; r++) // IPuzzle.Size = 9 - 0..8
        {
            for (int c = 0; c < IPuzzle.Size; c++) // IPuzzle.Size = 9 - 0..8
            {
                int mask = Puzzle[r, c].CenterCandidates.BitMask;
                if (mask == 0) continue;

                string text = GetCandidateString(mask);
                if (text.Length > IPuzzle.Size) text = text.Substring(0, IPuzzle.Size);

                double fontSize = GetFontSizeForCount(text.Length);

                var ft = new FormattedText(
                    text,
                    System.Globalization.CultureInfo.CurrentUICulture,
                    FlowDirection.LeftToRight,
                    new Typeface("Segoe UI"),
                    fontSize,
                    CenterMarkBrush,
                    1.0);

                // --- EINZELNE ZEICHEN EINFÄRBEN ---
                for (int i = 0; i < text.Length; i++)
                {
                    // Extrahiere die Ziffer an Position i
                    if (int.TryParse(text[i].ToString(), out int digit))
                    {
                        // Prüfen, ob genau diese Ziffer in dieser Zelle ein Konflikt ist
                        if (Puzzle[r, c].ConflictedCandidates.Contains(digit))
                        {
                            // Färbe nur dieses eine Zeichen rot
                            ft.SetForegroundBrush(Brushes.DarkRed, i, 1);
                        }
                    }
                }
                // ----------------------------------
                // zentriert
                double x = c * _cellSize + (_cellSize - ft.Width) / 2;

                double cellTop = r * _cellSize;
                double cellCenterY = cellTop + _cellSize / 2;
                double ascender = ft.Baseline;
                double opticalCorrection = -1;

                if (_cellSize > 52)
                {
                    if (fontSize < 12)
                        opticalCorrection = fontSize * 0.1;
                    else if (fontSize < 16)
                        opticalCorrection = fontSize * 0.05;
                }
                else
                {
                    if (fontSize < 10)
                        opticalCorrection = fontSize * 0.1;
                    else if (fontSize < 13)
                        opticalCorrection = fontSize * 0.05;
                }

                double y = cellCenterY - ascender / 2 + opticalCorrection;

                dc.DrawText(ft, new Point(x, y));
            }
        }
    }
    private void DrawCornerMarks()
    {
        using var dc = _cornerMarksVisual.RenderOpen();

        if (Puzzle == null) return;

        Span<int> dummy = stackalloc int[9];

        for (int r = 0; r < 9; r++)
        {
            for (int c = 0; c < 9; c++)
            {
                int mask = Puzzle[r, c].CornerCandidates.BitMask;
                if (mask == 0)
                    continue;

                var candidateCount = GetCandidates(mask, dummy);
                if (candidateCount == 0)
                    continue;

                double fontSize = GetCornerFontSize();

                for (int i = 0; i < candidateCount; i++)
                {
                    int value = dummy[i];
                    Point rel = CornerMarkPositions[i];

                    string text = value.ToString();

                    Brush brush = CenterMarkBrush;
                    if (Puzzle[r, c].ConflictedCandidates.Contains(value))
                    {
                        brush = Brushes.DarkRed;
                    }

                    var ft = new FormattedText(
                        text,
                        System.Globalization.CultureInfo.CurrentUICulture,
                        FlowDirection.LeftToRight,
                        new Typeface("Segoe UI"),
                        fontSize,
                        brush,
                        1.0);

                    double cellX = c * _cellSize;
                    double cellY = r * _cellSize;

                    double x = cellX + rel.X * _cellSize - ft.Width / 2;
                    double y = cellY + rel.Y * _cellSize - ft.Height / 2;

                    dc.DrawText(ft, new Point(x, y));
                }
            }
        }
    }

    private void DrawSolverMarks()
    {
        using var dc = _solverMarksVisual.RenderOpen();

        if (Puzzle == null) return;

        Span<int> dummy = stackalloc int[9];

        for (int r = 0; r < 9; r++)
        {
            for (int c = 0; c < 9; c++)
            {
                int mask = Puzzle[r, c].SolverCandidates.BitMask;
                if (mask == 0)
                    continue;

                var candidateCount = GetCandidates(mask, dummy);
                if (candidateCount == 0)
                    continue;

                double fontSize = GetCornerFontSize();

                for (int i = 0; i < candidateCount; i++)
                {
                    int value = dummy[i];
                    Point rel = SolverMarkPositions[i];

                    string text = value.ToString();

                    var ft = new FormattedText(
                        text,
                        System.Globalization.CultureInfo.CurrentUICulture,
                        FlowDirection.LeftToRight,
                        new Typeface("Segoe UI"),
                        fontSize,
                        CenterMarkBrush,
                        1.0);

                    double cellX = c * _cellSize;
                    double cellY = r * _cellSize;

                    double x = cellX + rel.X * _cellSize - ft.Width / 2;
                    double y = cellY + rel.Y * _cellSize - ft.Height / 2;

                    dc.DrawText(ft, new Point(x, y));
                }
            }
        }
    }
    private void DrawDigits()
    {
        using var dc = _digitsVisual.RenderOpen();

        if (Puzzle == null) return;

        for (int r = 0; r < IPuzzle.Size; r++)
        {
            for (int c = 0; c < IPuzzle.Size; c++)
            {
                int digit = Puzzle[r, c].Digit;

                if (digit == 0)
                    continue;

                string text = digit.ToString();
                double fontSize = _cellSize * 0.7;

                var ft = new FormattedText(
                    text,
                    System.Globalization.CultureInfo.CurrentUICulture,
                    FlowDirection.LeftToRight,
                    new Typeface("Segoe UI"),
                    fontSize,
                    Puzzle[r, c].IsGiven ? Brushes.Black : Brushes.Blue,
                    1.0);
                double x = c * _cellSize + (_cellSize - ft.Width) / 2;
                double y = r * _cellSize + (_cellSize - ft.Height) / 2;
                dc.DrawText(ft, new Point(x, y - 0.7));
            }
        }
    }
    private void DrawConflictHighlights()
    {
        using var dc = _conflictHighlightsVisual.RenderOpen();

        if (Puzzle == null) return;

        for (int r = 0; r < IPuzzle.Size; r++)
        {
            for (int c = 0; c < IPuzzle.Size; c++)
            {
                if (Puzzle[r, c].Digit == 0) continue;
                if (!Puzzle[r, c].IsConflicting) continue;

                double x = c * _cellSize;
                double y = r * _cellSize;

                Color bgColor = Colors.DarkRed;
                Color transparentColor = Color.FromArgb(128, bgColor.R, bgColor.G, bgColor.B);
                dc.DrawRectangle(new SolidColorBrush(transparentColor), null, new Rect(x, y, _cellSize, _cellSize));
            }
        }
    }
    private double GetCornerFontSize()
    {
        return _cellSize * 0.3;
    }
    private double GetFontSizeForCount(int count)
    {
        double baseSize = _cellSize * 0.3; // für 5 Zeichen
        if (count <= 5)
            return baseSize;
        if (count <= 7)
            return baseSize * 0.8;
        return baseSize * 0.6;
    }
    private static string GetCandidateString(int mask)
    {
        char[] chars = new char[9];
        int count = 0;
        for (int i = 0; i < 9 && count < 9; i++)
        {
            if ((mask & (1 << i)) != 0)
            {
                chars[count++] = (char)('1' + i);
            }
        }
        return new string(chars, 0, count);
    }
    private static int GetCandidates(int mask, Span<int> output)
    {
        int count = 0;
        for (int i = 0; i < 9; i++)
        {
            if ((mask & (1 << i)) != 0)
                output[count++] = i + 1;
        }
        return count;
    }
    private TransformedBitmap? GeneratePreview()
    {
        if (ActualWidth <= 0 || ActualHeight <= 0)
            return null;

        var dpi = VisualTreeHelper.GetDpi(this);

        // 1️⃣ Render in Originalgröße, nur die Layer ohne Selection
        var rtb = new RenderTargetBitmap(
            (int)(ActualWidth * dpi.DpiScaleX),
            (int)(ActualHeight * dpi.DpiScaleY),
            dpi.PixelsPerInchX,
            dpi.PixelsPerInchY,
            PixelFormats.Pbgra32);

        // Temporär AntiAliasing-Modi setzen
        RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);
        RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.HighQuality);

        // Rendern: alle Kinder außer _selectionVisual
        foreach (var visual in new Visual[] { _cellsVisual, _cellColorsVisual, _centerMarksVisual, _cornerMarksVisual, _digitsVisual, _gridVisual })
        {
            rtb.Render(visual);
        }

        // 2️⃣ Skalieren auf 300x300
        var scaled = new TransformedBitmap(rtb,
            new ScaleTransform(300.0 / rtb.PixelWidth, 300.0 / rtb.PixelHeight));

        return scaled;
    }
    #endregion

    #region Mouse & Keyboard Handling
    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        base.OnMouseDown(e);
        Focus();

        if (_cellSize <= 0) return;

        Point p = e.GetPosition(this);
        int col = (int)(p.X / _cellSize);
        int row = (int)(p.Y / _cellSize);

        if (row < 0 || row > 8 || col < 0 || col > 8) return;

        bool isCtrl = ActiveModifiers.HasFlag(ModifierKeys.Control);

        if (!isCtrl)
        {
            _selectedCells.Clear();
            ClearPublishedSelection();
        }

        if (isCtrl)
        {
            if (!_selectedCells.Add((row, col)))
                _selectedCells.Remove((row, col));
        }
        else
        {
            _selectedCells.Add((row, col));
        }

        _isMouseDown = true;

        DrawSelection();
        PublishSelectedCells();
    }
    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        if (!_isMouseDown) return;

        Point p = e.GetPosition(this);
        int col = (int)(p.X / _cellSize);
        int row = (int)(p.Y / _cellSize);

        if (row < 0 || row > 8 || col < 0 || col > 8) return;

        _selectedCells.Add((row, col));

        DrawSelection();
        PublishSelectedCells();
    }
    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
        base.OnMouseUp(e);
        _isMouseDown = false;
    }
    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (IsLocked) return;

        base.OnKeyDown(e);

        if (e.Key == Key.LeftShift || e.Key == Key.RightShift ||
            e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
            return;

        if (_selectedCells.Count == 0)
            return;

        int row = _selectedCell?.row ?? _selectedCells.First().row;
        int col = _selectedCell?.col ?? _selectedCells.First().col;

        int? digit = null;
        // Handling für Zifferntasten (D1-D9) und Numpad (NumPad1-NumPad9)
        if (e.Key >= Key.D1 && e.Key <= Key.D9) digit = e.Key - Key.D0;

        if (digit.HasValue)
        {
            string parameter = digit.Value.ToString();
            if (DigitInputAsIntCommand?.CanExecute(parameter) == true)
            {
                DigitInputAsIntCommand.Execute(parameter);

                IsFilled = InputActionHelper.IsFilledOnInputAction(Puzzle, InputActionType, GameMode);
            }
        }

        switch (e.Key)
        {
            case Key.Up:
                MoveSelection(row - 1, col);
                break;
            case Key.Down:
                MoveSelection(row + 1, col);
                break;
            case Key.Left:
                MoveSelection(row, col - 1);
                break;
            case Key.Right:
                MoveSelection(row, col + 1);
                break;
            default:
                return; // andere Keys ignorieren
        }
        e.Handled = true; // verhindert dass das KeyEvent woanders verarbeitet wird
    }
    private void MoveSelection(int row, int col)
    {

        row = Math.Clamp(row, 0, 8);
        col = Math.Clamp(col, 0, 8);

        _selectedCells.Clear();
        _selectedCells.Add((row, col));
        _selectedCell = (row, col);

        DrawSelection();
        PublishSelectedCells();
    }
    private void ClearPublishedSelection()
    {
        for (int r = 0; r < 9; r++)
            for (int c = 0; c < 9; c++)
                Puzzle[r, c].IsSelected = false;
    }
    private void PublishSelectedCells()
    {
        if (Puzzle == null)
            return;

        ClearPublishedSelection();

        foreach (var (r, c) in _selectedCells)
            Puzzle[r, c].IsSelected = true;
    }

    #endregion

}