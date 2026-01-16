using Avalonia.Controls;
using JamBox.Core.Services.Interfaces;
using JamBox.Core.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Reactive.Linq;

namespace JamBox.Core.Services;

public class NavigationService : INavigationService
{
    private readonly IServiceProvider _serviceProvider;

    private readonly Stack<UserControl> _navigationStack = [];

    private MainViewModel? _mainViewModel;

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
            throw new InvalidOperationException("MainViewModel not set");

        if (_mainViewModel.CurrentContent != null)
            _navigationStack.Push(_mainViewModel.CurrentContent);

        var view = _serviceProvider.GetRequiredService<TView>();
        _mainViewModel.SetCurrentContent(view);
    }

    public void NavigateTo<TView, TViewModel>()
        where TView : UserControl
        where TViewModel : class
    {
        if (_mainViewModel == null)
            throw new InvalidOperationException("MainViewModel not set");

        if (_mainViewModel.CurrentContent != null)
            _navigationStack.Push(_mainViewModel.CurrentContent);

        var view = _serviceProvider.GetRequiredService<TView>();
        var viewModel = _serviceProvider.GetRequiredService<TViewModel>();

        view.DataContext = viewModel;
        _mainViewModel.SetCurrentContent(view);
    }

    public void NavigateBack()
    {
        if (_mainViewModel == null)
            throw new InvalidOperationException("MainViewModel not set");

        if (_navigationStack.Count > 0)
        {
            var previousView = _navigationStack.Pop();
            _mainViewModel.SetCurrentContent(previousView);
        }
    }

    public void ToggleMiniPlayer()
    {
        _mainViewModel?.ToggleMiniPlayerCommand.Execute().Subscribe();
    }
}