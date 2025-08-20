using JamBox.Jellyfin; // Assuming your JellyfinApiService is in JamBox.Jellyfin namespace
using JamBox.JellyFin;
using ReactiveUI;
using System.Collections.ObjectModel; // For ObservableCollection
using System.Reactive;

namespace JamBox.Core.ViewModels
{
    public class MainViewModel : ReactiveObject
    {
        private readonly JellyfinApiService _jellyfinApiService;

        // Properties for UI binding
        private string _serverUrl = "http://192.168.68.100:8096"; // Default to your local IP
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

        // Use ObservableCollection for lists that the UI needs to update dynamically
        private ObservableCollection<BaseItemDto> _userLibraries = new ObservableCollection<BaseItemDto>();
        public ObservableCollection<BaseItemDto> UserLibraries
        {
            get => _userLibraries;
            set => this.RaiseAndSetIfChanged(ref _userLibraries, value);
        }

        private bool _isLoading = false;
        public bool IsLoading
        {
            get => _isLoading;
            set => this.RaiseAndSetIfChanged(ref _isLoading, value);
        }

        // Command for the Connect button
        public ReactiveCommand<Unit, Unit> ConnectCommand { get; }

        public MainViewModel()
        {
            _jellyfinApiService = new JellyfinApiService();

            // Initialize the ConnectCommand.
            // It's enabled when ServerUrl, Username, and Password are not empty, and not already loading.
            ConnectCommand = ReactiveCommand.CreateFromTask(ConnectToJellyfinAsync,
                this.WhenAnyValue(x => x.ServerUrl, x => x.Username, x => x.Password, x => x.IsLoading,
                                  (url, user, pass, loading) =>
                                      !string.IsNullOrWhiteSpace(url) &&
                                      !string.IsNullOrWhiteSpace(user) &&
                                      !string.IsNullOrWhiteSpace(pass) &&
                                      !loading));
        }

        private async Task ConnectToJellyfinAsync()
        {
            IsLoading = true;
            ConnectionStatus = "Attempting to connect...";
            ServerInfo = null;
            UserLibraries.Clear(); // Clear previous libraries

            _jellyfinApiService.SetServerUrl(ServerUrl); // Update service URL with current UI input

            try
            {
                // Step 1: Get public info
                var publicInfo = await _jellyfinApiService.GetPublicSystemInfoAsync();
                if (publicInfo == null)
                {
                    ConnectionStatus = "Connection failed: Could not get public server info. Check URL.";
                    return;
                }
                ServerInfo = publicInfo;
                ConnectionStatus = $"Found Jellyfin server: {publicInfo.ServerName} (v{publicInfo.Version}). Authenticating...";

                // Step 2: Authenticate
                var isAuthenticated = await _jellyfinApiService.AuthenticateUserAsync(Username, Password);
                if (!isAuthenticated)
                {
                    ConnectionStatus = "Authentication failed. Check username and password.";
                    return;
                }
                ConnectionStatus = "Authentication successful! Fetching libraries...";

                // Step 3: Get user libraries
                var libraries = await _jellyfinApiService.GetUserMediaViewsAsync();
                if (libraries == null)
                {
                    ConnectionStatus = "Failed to fetch user libraries.";
                    return;
                }
                // Populate ObservableCollection for UI update
                foreach (var lib in libraries)
                {
                    UserLibraries.Add(lib);
                }
                ConnectionStatus = "Successfully connected and loaded libraries!";
            }
            catch (Exception ex)
            {
                // Log the full exception for debugging
                Console.WriteLine($"An unexpected error occurred during Jellyfin connection: {ex}");
                ConnectionStatus = $"An unexpected error occurred: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}