using Avalonia;
using Avalonia.Controls;
using JamBox.Core.Settings;
using JamBox.Core.ViewModels;

namespace JamBox.Core.Views;

public partial class MainWindow : Window
{
    private readonly WindowSettings _settings;

    public MainWindow(MainViewModel mainViewModel)
    {
        InitializeComponent();
        DataContext = mainViewModel;
        _settings = WindowSettings.Load();

        Width = _settings.Width;
        Height = _settings.Height;

        if (_settings.X.HasValue && _settings.Y.HasValue)
        {
            Position = new PixelPoint((int)_settings.X.Value, (int)_settings.Y.Value);
        }

        this.Closing += OnClosing;
    }

    private void OnClosing(object? sender, WindowClosingEventArgs e)
    {
        _settings.Width = Width;
        _settings.Height = Height;
        _settings.X = Position.X;
        _settings.Y = Position.Y;
        _settings.Save();
    }
}