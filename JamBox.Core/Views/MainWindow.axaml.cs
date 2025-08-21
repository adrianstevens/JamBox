using Avalonia.Controls;
using JamBox.Core.JellyFin;
using JamBox.Core.ViewModels;

namespace JamBox.Core.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow(JellyfinApiService jellyfinApiService)
        {
            InitializeComponent();
            DataContext = new MainViewModel(jellyfinApiService);
        }
    }
}