using Avalonia.Data.Converters;
using JamBox.Core.Models;
using System.Globalization;

namespace JamBox.Core.Converters;

/// <summary>
/// Compares a track's Id with the currently playing track's Id.
/// Returns true if they match (indicating this is the currently playing track).
/// </summary>
public class IsCurrentTrackConverter : IMultiValueConverter
{
    public static readonly IsCurrentTrackConverter Instance = new();

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        // values[0] = current item's Track
        // values[1] = PlaybackViewModel.SelectedTrack (currently playing)
        if (values.Count < 2)
            return false;

        var currentTrack = values[0] as Track;
        var playingTrack = values[1] as Track;

        if (currentTrack == null || playingTrack == null)
            return false;

        return currentTrack.Id == playingTrack.Id;
    }
}
