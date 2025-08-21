using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using JamBox.Core.JellyFin;
using JamBox.Core.Views;

namespace JamBox.Core
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            var jellyfinService = new JellyfinApiService(); // Only one instance
            jellyfinService.SetServerUrl("http://192.168.68.100:8096");

            var mainWindow = new MainWindow(jellyfinService);


            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = mainWindow;
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}