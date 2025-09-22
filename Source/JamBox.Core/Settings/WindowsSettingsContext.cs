using JamBox.Core.Settings;
using System.Text.Json.Serialization;

namespace JamBox.Core;

[JsonSerializable(typeof(WindowSettings))]
public partial class WindowSettingsContext : JsonSerializerContext
{ }