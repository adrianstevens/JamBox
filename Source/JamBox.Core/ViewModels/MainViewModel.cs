using Avalonia.Controls;
using JamBox.Core.Services.Interfaces;
using JamBox.Core.Views;
using ReactiveUI;
using System.Reactive;

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

    private bool _isMiniPlayerMode;
    public bool IsMiniPlayerMode
    {
        get => _isMiniPlayerMode;
        set => this.RaiseAndSetIfChanged(ref _isMiniPlayerMode, value);
    }

    public ReactiveCommand<Unit, Unit> ToggleMiniPlayerCommand { get; }

    /// <summary>
    /// Event raised when mini player mode is toggled.
    /// The MainWindow subscribes to this to resize accordingly.
    /// </summary>
    public event EventHandler<bool>? MiniPlayerModeChanged;

    public MainViewModel(
        INavigationService navigationService,
        IJellyfinApiService jellyfinApiService)
    {
        _navigationService = navigationService;

        _navigationService.SetMainViewModel(this);

        ToggleMiniPlayerCommand = ReactiveCommand.Create(ToggleMiniPlayerMode);

        _navigationService.NavigateTo<LoginPage, LoginViewModel>();
    }

    public void SetCurrentContent(UserControl content)
    {
        CurrentContent = content;
    }

    private void ToggleMiniPlayerMode()
    {
        IsMiniPlayerMode = !IsMiniPlayerMode;
        MiniPlayerModeChanged?.Invoke(this, IsMiniPlayerMode);
    }
}