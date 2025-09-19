using JamBox.Core.Services.Interfaces;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive;

namespace JamBox.Core.ViewModels;

public class JukeBoxViewModel
{
    private readonly INavigationService _navigationService;

    public ObservableCollection<string> Albums { get; } = [];

    public ObservableCollection<string> Tracks { get; } = [];

    public ObservableCollection<string> Playlist { get; } = [];

    public ReactiveCommand<Unit, Unit> GoBackCommand { get; }

    public JukeBoxViewModel(
        INavigationService navigationService)
    {
        _navigationService = navigationService;


        Albums.Add("Album 1");
        Albums.Add("Album 2");
        Albums.Add("Album 3");
        Albums.Add("Album 4");
        Albums.Add("Album 5");
        Albums.Add("Album 6");
        Albums.Add("Album 7");
        Albums.Add("Album 8");


        Tracks.Add("Track 1");
        Tracks.Add("Track 2");
        Tracks.Add("Track 3");
        Tracks.Add("Track 4");
        Tracks.Add("Track 5");
        Tracks.Add("Track 6");
        Tracks.Add("Track 7");
        Tracks.Add("Track 8");


        Playlist.Add("Track A");
        Playlist.Add("Track B");
        Playlist.Add("Track C");
        Playlist.Add("Track D");
        Playlist.Add("Track E");
        Playlist.Add("Track F");
        Playlist.Add("Track G");
        Playlist.Add("Track H");
        Playlist.Add("Track I");

        GoBackCommand = ReactiveCommand.Create(() =>
        {
            _navigationService.NavigateBack();
        });
    }
}