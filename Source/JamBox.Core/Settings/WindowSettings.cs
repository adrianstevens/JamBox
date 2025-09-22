using System.Text.Json;

namespace JamBox.Core.Settings;

public class WindowSettings
{
    public double Width { get; set; } = 1200;

    public double Height { get; set; } = 800;

    public double? X { get; set; }

    public double? Y { get; set; }

    private static string SettingsPath =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "JamBox", "windowsettings.json");

    public static WindowSettings Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                var json = File.ReadAllText(SettingsPath);
                return JsonSerializer.Deserialize(json, WindowSettingsContext.Default.WindowSettings) ?? new WindowSettings();
            }
        }
        catch { }
        return new WindowSettings();
    }

    public void Save()
    {
        try
        {
            var dir = Path.GetDirectoryName(SettingsPath);
            if (!Directory.Exists(dir!))
                Directory.CreateDirectory(dir!);

            var json = JsonSerializer.Serialize(this, WindowSettingsContext.Default.WindowSettings);
            File.WriteAllText(SettingsPath, json);
        }
        catch { }
    }
}