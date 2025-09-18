using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using JamBox.Core.Audio;
using JamBox.Core.Services;
using JamBox.Core.Services.Interfaces;
using JamBox.Core.ViewModels;
using JamBox.Core.Views;
using Microsoft.Extensions.DependencyInjection;

namespace JamBox.Core;

public partial class App : Application
{
    private ServiceProvider? _serviceProvider;
    private TrayIcon? _tray;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
#if DEBUG
        this.AttachDevTools();
#endif
        var services = new ServiceCollection();

        services.AddSingleton<IAudioPlayer, AudioPlayer>();
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IJellyfinApiService, JellyfinApiService>();

        services.AddSingleton<MainViewModel>();
        services.AddTransient<LoginViewModel>();
        services.AddTransient<LibraryViewModel>();

        services.AddSingleton<MainWindow>();
        services.AddTransient<LoginPage>();
        services.AddTransient<LibraryPage>();
        services.AddTransient<JukeBoxPage>();

        _serviceProvider = services.BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Keep the process alive even when all windows are hidden.
            desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            desktop.MainWindow = mainWindow;

            // Hide to tray when minimized
            mainWindow.GetObservable(Window.WindowStateProperty).Subscribe(state =>
            {
                if (state == WindowState.Minimized)
                {
                    mainWindow.Hide();
                }
            });

            _tray = new TrayIcon
            {
                Icon = new WindowIcon(AssetLoader.Open(new Uri("avares://JamBox.Core/Assets/appicon.ico"))),
                ToolTipText = "JamBox"
            };

            _tray.Clicked += (_, __) =>
            {
                mainWindow.Show();
                mainWindow.WindowState = WindowState.Normal;
                mainWindow.Activate();
            };

            var menu = new NativeMenu();
            var openItem = new NativeMenuItem("Open");
            openItem.Click += (_, __) =>
            {
                mainWindow.Show();
                mainWindow.WindowState = WindowState.Normal;
                mainWindow.Activate();
            };
            var sep = new NativeMenuItemSeparator();
            var exitItem = new NativeMenuItem("Exit");
            exitItem.Click += (_, __) => desktop.Shutdown();

            menu.Items.Add(openItem);
            menu.Items.Add(sep);
            menu.Items.Add(exitItem);

            _tray.Menu = menu;
            _tray.IsVisible = true;
        }

        base.OnFrameworkInitializationCompleted();
    }
}