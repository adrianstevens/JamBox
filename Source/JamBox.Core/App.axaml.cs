using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
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
        services.AddTransient<LoginView>();
        services.AddTransient<LibraryView>();

        _serviceProvider = services.BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            //desktop.Exit += (_, __) => mainViewModel.Dispose();
        }

        base.OnFrameworkInitializationCompleted();
    }
}