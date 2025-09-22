using Avalonia.Controls;
using JamBox.Core.Services.Interfaces;
using JamBox.Core.Views;
using ReactiveUI;

namespace JamBox.Core.ViewModels;

public class MainViewModel : ReactiveObject
{
    private readonly INavigationService _navigationService;

    private UserControl? _currentContent;
    public UserControl? CurrentContent
    {
        get => _currentContent;
        set => this.RaiseAndSetIfChanged(ref _currentContent, value);
    }

    public MainViewModel(
        IAudioPlayer audioPlayer,
        INavigationService navigationService,
        IJellyfinApiService jellyfinApiService)
    {
        _navigationService = navigationService;

        _navigationService.SetMainViewModel(this);

        _navigationService.NavigateTo<LoginPage, LoginViewModel>();
    }

    public void SetCurrentContent(UserControl content)
    {
        CurrentContent = content;
    }
}