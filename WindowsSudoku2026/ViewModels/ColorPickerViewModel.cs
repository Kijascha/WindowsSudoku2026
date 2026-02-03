using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Windows.Media;
using WindowsSudoku2026.Common.Utils;
using WindowsSudoku2026.Core.ViewModels;

namespace WindowsSudoku2026.ViewModels;

public partial class ColorPickerViewModel : ViewModel
{
    [ObservableProperty] private Color _selectedColor;
    [ObservableProperty] private Color _finalColor;
    [ObservableProperty] private Hsv _selectedHsv;
    [ObservableProperty] private string _r;
    [ObservableProperty] private string _g;
    [ObservableProperty] private string _b;

    private Color _interpetedColor;

    public ColorPickerViewModel()
    {
        _selectedColor = Colors.White;
        _finalColor = Colors.White;
        _r = _finalColor.R.ToString();
        _g = _finalColor.G.ToString();
        _b = _finalColor.B.ToString();
    }
    partial void OnSelectedHsvChanged(Hsv value)
    {
    }
    partial void OnSelectedColorChanged(Color value)
    {
        R = _selectedColor.R.ToString();
        G = _selectedColor.G.ToString();
        B = _selectedColor.B.ToString();
    }

    partial void OnFinalColorChanged(Color value)
    {
        R = FinalColor.R.ToString();
        G = FinalColor.G.ToString();
        B = FinalColor.B.ToString();

        WeakReferenceMessenger.Default.Send(new ColorPickerMessage(FinalColor));
    }
    partial void OnRChanged(string value) => UpdateColorFromRgb();
    partial void OnGChanged(string value) => UpdateColorFromRgb();
    partial void OnBChanged(string value) => UpdateColorFromRgb();
    private void UpdateColorFromRgb()
    {
        if (byte.TryParse(R, out byte r) &&
            byte.TryParse(G, out byte g) &&
            byte.TryParse(B, out byte b))
        {
            _interpetedColor = Color.FromArgb(FinalColor.A, r, g, b);
        }
    }

    [RelayCommand]
    private void Enter()
    {
        SelectedColor = Color.FromRgb(_interpetedColor.R, _interpetedColor.G, _interpetedColor.B);
    }
}
public record ColorPickerMessage(Color newColor);