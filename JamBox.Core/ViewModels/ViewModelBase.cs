using ReactiveUI;

namespace JamBox.Core.ViewModels;

public class ViewModelBase : ReactiveObject
{
    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set => this.RaiseAndSetIfChanged(ref _isBusy, value);
    }
}
