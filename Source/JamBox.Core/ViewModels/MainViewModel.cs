using JamBox.Core.Services.Interfaces;
using ReactiveUI;

namespace JamBox.Core.ViewModels;

public class MainViewModel : ReactiveObject, IDisposable
{
    IAudioPlayer _player;
    IJellyfinApiService _jellyfinApiService;

    private ViewModelBase _currentContent;
    public ViewModelBase CurrentContent
    {
        get => _currentContent;
        set => this.RaiseAndSetIfChanged(ref _currentContent, value);
    }

    public MainViewModel(
        IAudioPlayer player,
        IJellyfinApiService jellyfinApiService)
    {
        _player = player;
        _jellyfinApiService = jellyfinApiService;

        CurrentContent = new LoginViewModel(this);
    }

    public void Dispose()
    {
        _player.Dispose();
    }
}