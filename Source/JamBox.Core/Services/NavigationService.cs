using Avalonia.Controls;
using JamBox.Core.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace JamBox.Core.Services;

public class NavigationService : INavigationService
{
    private readonly IServiceProvider _serviceProvider;
    private ContentControl _contentControl;

    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void SetContentControl(ContentControl contentControl)
    {
        _contentControl = contentControl;
    }

    public void NavigateTo<TView>() where TView : UserControl
    {
        if (_contentControl == null)
            throw new InvalidOperationException("ContentControl not set");

        var view = _serviceProvider.GetRequiredService<TView>();
        _contentControl.Content = view;
    }

    public void NavigateTo<TView, TViewModel>()
        where TView : UserControl
        where TViewModel : class
    {
        if (_contentControl == null)
            throw new InvalidOperationException("ContentControl not set");

        var view = _serviceProvider.GetRequiredService<TView>();
        var viewModel = _serviceProvider.GetRequiredService<TViewModel>();

        view.DataContext = viewModel;
        _contentControl.Content = view;
    }
}