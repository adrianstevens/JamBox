using Avalonia;
using Avalonia.Controls;
using JamBox.Core.Settings;
using JamBox.Core.ViewModels;
using JamBox.Core.Views.UserControls;

namespace JamBox.Core.Views;

public partial class MainWindow : Window
{
    private readonly WindowSettings _settings;
    private readonly MainViewModel _mainViewModel;
    private const double MiniPlayerSize = 300;

    // Store normal window dimensions when switching to mini mode
    private double _normalWidth;
    private double _normalHeight;
    private PixelPoint? _normalPosition;

    // Store reference to normal content when in mini mode
    private UserControl? _normalContent;
    private MiniPlayerView? _miniPlayerView;

    public MainWindow(MainViewModel mainViewModel)
    {
        InitializeComponent();
        _mainViewModel = mainViewModel;
        DataContext = mainViewModel;
        _settings = WindowSettings.Load();

        Width = _settings.Width;
        Height = _settings.Height;
        _normalWidth = _settings.Width;
        _normalHeight = _settings.Height;

        if (_settings.X.HasValue && _settings.Y.HasValue)
        {
            Position = new PixelPoint((int)_settings.X.Value, (int)_settings.Y.Value);
        }

        // Subscribe to mini player mode changes
        mainViewModel.MiniPlayerModeChanged += OnMiniPlayerModeChanged;

        Closing += OnClosing;
    }

    private void OnMiniPlayerModeChanged(object? sender, bool isMiniMode)
    {
        if (isMiniMode)
        {
            // Save current dimensions before switching to mini mode
            _normalWidth = Width;
            _normalHeight = Height;
            _normalPosition = Position;

            // Store reference to normal content and switch to mini player
            _normalContent = _mainViewModel.CurrentContent;
            
            // Create mini player view with the same DataContext as the current content
            _miniPlayerView = new MiniPlayerView
            {
                DataContext = _normalContent?.DataContext
            };
            _mainViewModel.SetCurrentContent(_miniPlayerView);

            // Switch to mini player dimensions
            MinWidth = MiniPlayerSize;
            MinHeight = MiniPlayerSize;
            MaxWidth = MiniPlayerSize;
            MaxHeight = MiniPlayerSize;
            Width = MiniPlayerSize;
            Height = MiniPlayerSize;
            CanResize = false;
        }
        else
        {
            // Restore normal content
            if (_normalContent != null)
            {
                _mainViewModel.SetCurrentContent(_normalContent);
            }
            _miniPlayerView = null;

            // Restore normal window dimensions
            MinWidth = 0;
            MinHeight = 0;
            MaxWidth = double.PositiveInfinity;
            MaxHeight = double.PositiveInfinity;
            CanResize = true;
            Width = _normalWidth;
            Height = _normalHeight;

            if (_normalPosition.HasValue)
            {
                Position = _normalPosition.Value;
            }
        }
    }

    private void OnClosing(object? sender, WindowClosingEventArgs e)
    {
        // Save normal dimensions, not mini player dimensions
        if (!_mainViewModel.IsMiniPlayerMode)
        {
            _settings.Width = Width;
            _settings.Height = Height;
        }
        else
        {
            _settings.Width = _normalWidth;
            _settings.Height = _normalHeight;
        }
        _settings.X = Position.X;
        _settings.Y = Position.Y;
        _settings.Save();
    }
}