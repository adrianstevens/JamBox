using Avalonia.Controls;
using Avalonia.Input;

namespace JamBox.Core.Views.UserControls;

public partial class MiniPlayerView : UserControl
{
    public MiniPlayerView()
    {
        InitializeComponent();
    }

    private void Seek_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is ViewModels.LibraryViewModel vm)
        {
            vm.Playback.IsUserSeeking = true;
        }
    }

    private void Seek_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (DataContext is ViewModels.LibraryViewModel vm)
        {
            vm.Playback.IsUserSeeking = false;
            vm.Playback.SeekPosition = SeekBar.Value;
        }
    }
}
