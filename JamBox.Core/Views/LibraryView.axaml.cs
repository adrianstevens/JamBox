using Avalonia.Controls;
using JamBox.Core.JellyFin;

namespace JamBox.Core.Views;

public partial class LibraryView : UserControl
{
    public LibraryView(JellyfinApiService jellyfinApiService)
    {
        InitializeComponent();
    }
}