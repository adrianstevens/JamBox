using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace JamBox.Core.Views.UserControls;

public partial class NowPlayingView : UserControl
{
    public NowPlayingView()
    {
        InitializeComponent();

        this.AttachedToVisualTree += (_, __) =>
        {
            var seek = this.FindControl<Slider>("SeekBar");
            if (seek is null) return;

            // Start seeking before the Thumb handles it
            seek.AddHandler(InputElement.PointerPressedEvent,
                Seek_OnPointerPressed,
                RoutingStrategies.Tunnel);

            // Commit on release (tunnel to be safe)...
            seek.AddHandler(InputElement.PointerReleasedEvent,
                Seek_OnPointerReleased,
                RoutingStrategies.Tunnel);

            // ...and also when capture is lost (mouse released outside, etc.)
            seek.AddHandler(InputElement.PointerCaptureLostEvent,
                Seek_OnPointerCaptureLost,
                RoutingStrategies.Bubble);
        };
    }

    private void Seek_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is ViewModels.PlaybackViewModel vm)
        {
            vm.IsUserSeeking = true;
        }
    }

    private void Seek_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (DataContext is ViewModels.PlaybackViewModel vm && sender is Slider s)
        {
            vm.IsUserSeeking = false;
            vm.SeekTo(s.Value);
        }
    }

    private void Seek_OnPointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        if (DataContext is ViewModels.PlaybackViewModel vm && sender is Slider s)
        {
            vm.IsUserSeeking = false;
            vm.SeekTo(s.Value);
        }
    }
}