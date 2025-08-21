using Avalonia.Controls;
using JamBox.Core.JellyFin;
using JamBox.Core.ViewModels;

namespace JamBox.Core.Views
{
    public partial class LibraryView : UserControl
    {
        public LibraryView(JellyfinApiService jellyfinApiService)
        {
            InitializeComponent();
            DataContext = new LibraryViewModel(jellyfinApiService);
        }
    }
}