using Avalonia;
using Avalonia.ReactiveUI;
using JamBox.Core;

namespace JamBox;

internal class Program
{
    public static void Main(string[] args) =>
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

    public static AppBuilder BuildAvaloniaApp()
    {
        var builder = AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .UseReactiveUI();
#if DEBUG
        builder = builder.LogToTrace();
#endif
        return builder;
    }
}