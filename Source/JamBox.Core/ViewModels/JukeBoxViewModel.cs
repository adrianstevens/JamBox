using JamBox.Core.Services.Interfaces;

namespace JamBox.Core.ViewModels;

public class JukeBoxViewModel
{
    private readonly INavigationService _navigationService;

    public JukeBoxViewModel(
        INavigationService navigationService)
    {
        _navigationService = navigationService;
    }

}
