using JamBox.Core.JellyFin;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;

namespace JamBox.Core.ViewModels;

public class LoginViewModel : ViewModelBase
{
    private readonly JellyfinApiService _jellyfinApiService;
    private readonly MainViewModel _mainViewModel;

    // UI Bound Properties
    private string _serverUrl = "http://192.168.68.100:8096";
    public string ServerUrl
    {
        get => _serverUrl;
        set => this.RaiseAndSetIfChanged(ref _serverUrl, value);
    }

    private string _username = "";
    public string Username
    {
        get => _username;
        set => this.RaiseAndSetIfChanged(ref _username, value);
    }

    private string _password = "";
    public string Password
    {
        get => _password;
        set => this.RaiseAndSetIfChanged(ref _password, value);
    }

    private string _connectionStatus = "Enter server details and connect.";
    public string ConnectionStatus
    {
        get => _connectionStatus;
        set => this.RaiseAndSetIfChanged(ref _connectionStatus, value);
    }

    private PublicSystemInfo _serverInfo;
    public PublicSystemInfo ServerInfo
    {
        get => _serverInfo;
        set => this.RaiseAndSetIfChanged(ref _serverInfo, value);
    }

    private ObservableCollection<BaseItemDto> _userLibraries = new ObservableCollection<BaseItemDto>();
    public ObservableCollection<BaseItemDto> UserLibraries
    {
        get => _userLibraries;
        set => this.RaiseAndSetIfChanged(ref _userLibraries, value);
    }

    // Command for the Connect button
    public ReactiveCommand<Unit, Unit> ConnectCommand { get; }

    public LoginViewModel(JellyfinApiService jellyfinService, MainViewModel mainViewModel)
    {
        _jellyfinApiService = jellyfinService;
        _mainViewModel = mainViewModel;

        // Connect Command: Enabled when inputs are valid and not loading
        ConnectCommand = ReactiveCommand.CreateFromTask(ConnectToJellyfinAsync, Observable.Return(true));
    }

    private async Task ConnectToJellyfinAsync()
    {
        ConnectionStatus = "Attempting to connect...";
        ServerInfo = null;
        UserLibraries.Clear();

        _jellyfinApiService.SetServerUrl(ServerUrl);

        try
        {
            var publicInfo = await _jellyfinApiService.GetPublicSystemInfoAsync();

            if (publicInfo == null)
            {
                ConnectionStatus = "Connection failed: Could not get public server info. Check URL or connectivity.";
                return;
            }

            ServerInfo = publicInfo;
            ConnectionStatus = $"Found Jellyfin server: {publicInfo.ServerName} (v{publicInfo.Version}). Authenticating...";

            var isAuthenticated = await _jellyfinApiService.AuthenticateUserAsync(Username, Password);
            if (!isAuthenticated)
            {
                ConnectionStatus = "Authentication failed. Check username and password.";
                return;
            }

            ConnectionStatus = "Authentication successful! Fetching libraries...";

            var libraries = await _jellyfinApiService.GetUserMediaViewsAsync();
            if (libraries == null)
            {
                ConnectionStatus = "Failed to fetch user libraries.";
                return;
            }

            UserLibraries.Clear();
            foreach (var lib in libraries)
            {
                UserLibraries.Add(lib);
            }

            ConnectionStatus = "Successfully connected and loaded libraries!";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An unexpected error occurred during Jellyfin connection: {ex}");
            ConnectionStatus = $"An unexpected error occurred: {ex.Message}";
        }
    }
}