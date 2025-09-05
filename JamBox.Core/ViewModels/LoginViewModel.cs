using JamBox.Core.JellyFin;
using JamBox.Core.Models;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Linq;
using System.Text.Json;

namespace JamBox.Core.ViewModels;

public class LoginViewModel : ViewModelBase
{
    private static string CredentialsPath =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "JamBox", "credentials.json");

    private readonly JellyfinApiService _jellyfinApiService;
    private readonly MainViewModel _mainViewModel;

    private string _serverUrl = "http://192.168.50.157:8096/jellyfin/";
    public string ServerUrl
    {
        get => _serverUrl;
        set => this.RaiseAndSetIfChanged(ref _serverUrl, value);
    }

    private string _username = "jorgedevs";
    public string Username
    {
        get => _username;
        set => this.RaiseAndSetIfChanged(ref _username, value);
    }

    private string _password = "pass";
    public string Password
    {
        get => _password;
        set => this.RaiseAndSetIfChanged(ref _password, value);
    }

    private string _connectionStatus = string.Empty;
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

    public ReactiveCommand<Unit, Unit> ConnectCommand { get; }

    public LoginViewModel(JellyfinApiService jellyfinService, MainViewModel mainViewModel)
    {
        _jellyfinApiService = jellyfinService;
        _mainViewModel = mainViewModel;

        var canConnect = this.WhenAnyValue(
            x => x.ServerUrl,
            x => x.Username,
            x => x.Password,
            (server, user, password) =>
                !string.IsNullOrWhiteSpace(server) &&
                !string.IsNullOrWhiteSpace(user) &&
                !string.IsNullOrWhiteSpace(password));
        ConnectCommand = ReactiveCommand.CreateFromTask(ConnectToJellyfinAsync, canConnect);

        ConnectCommand.ThrownExceptions.Subscribe(ex =>
        {
            Console.WriteLine($"An unhandled error occurred in the ConnectCommand: {ex.Message}");
            ConnectionStatus = $"Connection failed: {ex.Message}";
        });

        //LoadCredentials();
        ConnectCommand.Execute().Subscribe();
    }

    private void SaveCredentials()
    {
        var creds = new UserCredentials
        {
            ServerUrl = ServerUrl,
            Username = Username,
            Password = Password
        };

        var dir = Path.GetDirectoryName(CredentialsPath);

        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir!);
        }

        File.WriteAllText(CredentialsPath, JsonSerializer.Serialize(creds));
    }

    private void LoadCredentials()
    {
        if (File.Exists(CredentialsPath))
        {
            var creds = JsonSerializer.Deserialize<UserCredentials>(File.ReadAllText(CredentialsPath));

            if (creds != null)
            {
                ServerUrl = creds.ServerUrl;
                Username = creds.Username;
                Password = creds.Password;
            }

            ConnectCommand.Execute().Subscribe();
        }
    }

    private async Task ConnectToJellyfinAsync()
    {
        IsBusy = true;

        ConnectionStatus = "Attempting to connect...";
        ServerInfo = null;

        _jellyfinApiService.SetServerUrl(ServerUrl);

        try
        {
            var publicInfo = await _jellyfinApiService.GetPublicSystemInfoAsync();
            if (publicInfo == null)
            {
                ConnectionStatus = "Connection failed: Could not get public server info. Check URL or connectivity.";
                IsBusy = false;
                return;
            }

            ServerInfo = publicInfo;
            ConnectionStatus = $"Found Jellyfin server: {publicInfo.ServerName} (v{publicInfo.Version}). Authenticating...";

            var isAuthenticated = await _jellyfinApiService.AuthenticateUserAsync(Username, Password);
            if (!isAuthenticated)
            {
                ConnectionStatus = "Authentication failed. Check username and password.";
                IsBusy = false;
                return;
            }

            SaveCredentials();
            ConnectionStatus = "Authentication successful!";

            await Task.Delay(1000);

            _mainViewModel.CurrentContent = new LibraryViewModel(_jellyfinApiService);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An unexpected error occurred during Jellyfin connection: {ex}");
            ConnectionStatus = $"An unexpected error occurred: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}