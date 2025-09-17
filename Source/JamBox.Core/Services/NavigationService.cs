using Avalonia.Controls;
using JamBox.Core.Services.Interfaces;
using JamBox.Core.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace JamBox.Core.Services;

public class NavigationService : INavigationService
{
    private readonly IServiceProvider _serviceProvider;
    private MainViewModel _mainViewModel;

    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void SetMainViewModel(MainViewModel mainViewModel)
    {
        _mainViewModel = mainViewModel;
    }

    public void NavigateTo<TView>()
        where TView : UserControl
    {
        if (_mainViewModel == null)
        {
            throw new InvalidOperationException("MainViewModel not set");
        }

        var view = _serviceProvider.GetRequiredService<TView>();
        _mainViewModel.SetCurrentContent(view);
    }

    public void NavigateTo<TView, TViewModel>()
        where TView : UserControl
        where TViewModel : class
    {
        if (_mainViewModel == null)
        {
            throw new InvalidOperationException("MainViewModel not set");
        }

        var view = _serviceProvider.GetRequiredService<TView>();
        var viewModel = _serviceProvider.GetRequiredService<TViewModel>();

        view.DataContext = viewModel;
        _mainViewModel.SetCurrentContent(view);
    }
}