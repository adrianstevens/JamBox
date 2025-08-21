using JamBox.Core.JellyFin;
using ReactiveUI;

namespace JamBox.Core.ViewModels;

public class MainViewModel : ReactiveObject
{
    private readonly JellyfinApiService _jellyfinApiService;
    private ViewModelBase _currentContent;

    public ViewModelBase CurrentContent
    {
        get => _currentContent;
        // The setter is now public to allow other view models to update it
        set => this.RaiseAndSetIfChanged(ref _currentContent, value);
    }

    public MainViewModel(JellyfinApiService jellyfinService)
    {
        _jellyfinApiService = jellyfinService;

        // Set the initial content to the LoginViewModel, passing 'this' for navigation.
        CurrentContent = new LoginViewModel(_jellyfinApiService, this);
    }
}