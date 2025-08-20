using Avalonia.Controls;
using JamBox.Core.ViewModels;

namespace JamBox.Core.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
    }
}