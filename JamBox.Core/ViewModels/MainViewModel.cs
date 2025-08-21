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
        private set => this.RaiseAndSetIfChanged(ref _currentContent, value);
    }

    public MainViewModel(JellyfinApiService jellyfinService)
    {
        _jellyfinApiService = jellyfinService;

        // Set the initial content to the LoginViewModel
        CurrentContent = new LoginViewModel(_jellyfinApiService, this);
    }

    // Method to handle navigation to the library view
    public void NavigateToLibrary()
    {
        CurrentContent = new LibraryViewModel(_jellyfinApiService);
    }
}