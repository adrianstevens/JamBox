namespace JamBox.Core.Services.Interfaces;

public enum PlaybackState { Stopped, Playing, Paused }

public interface IAudioPlayerService : IDisposable
{
    PlaybackState State { get; }

    long PositionMs { get; }

    long LengthMs { get; }

    int Volume { get; set; } //0-100 

    event EventHandler<int>? VolumeChanged;

    event EventHandler<long>? PositionChanged;

    event EventHandler<PlaybackState>? StateChanged;

    Task PlayAsync(string url, IDictionary<string, string>? headers = null);

    void Pause();

    void Resume();

    void Stop();

    void Seek(long positionMs);
}