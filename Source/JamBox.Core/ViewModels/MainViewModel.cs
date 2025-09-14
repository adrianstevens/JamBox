using JamBox.Core.Audio;
using JamBox.Core.JellyFin;
using ReactiveUI;

namespace JamBox.Core.ViewModels;

public class MainViewModel : ReactiveObject, IDisposable
{
    private readonly JellyfinApiService _jellyfinApiService;
    private ViewModelBase _currentContent;

    public IAudioPlayer Player { get; } = new LibVlcAudioPlayer();

    public ViewModelBase CurrentContent
    {
        get => _currentContent;
        set => this.RaiseAndSetIfChanged(ref _currentContent, value);
    }

    public MainViewModel(JellyfinApiService jellyfinService)
    {
        _jellyfinApiService = jellyfinService;
        CurrentContent = new LoginViewModel(_jellyfinApiService, this);
    }

    public void Dispose()
    {
        Player.Dispose();
    }
}