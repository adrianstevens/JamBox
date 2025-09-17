using Avalonia.Controls;

namespace JamBox.Core.Services.Interfaces;

public interface INavigationService
{
    void NavigateTo<TView>() where TView : UserControl;

    void NavigateTo<TView, TViewModel>() where TView : UserControl where TViewModel : class;

    void SetContentControl(ContentControl contentControl);
}
