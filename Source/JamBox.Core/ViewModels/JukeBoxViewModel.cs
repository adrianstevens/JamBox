using JamBox.Core.Services.Interfaces;
using ReactiveUI;
using System.Reactive;

namespace JamBox.Core.ViewModels;

public class JukeBoxViewModel
{
    private readonly INavigationService _navigationService;

    public ReactiveCommand<Unit, Unit> GoBackCommand { get; }

    public JukeBoxViewModel(
        INavigationService navigationService)
    {
        _navigationService = navigationService;

        GoBackCommand = ReactiveCommand.Create(() =>
        {
            _navigationService.NavigateBack();
        });
    }
}