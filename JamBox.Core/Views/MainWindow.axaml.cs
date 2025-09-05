using Avalonia.Controls;

namespace JamBox.Core.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Width = 1200;   // Set your preferred width
        Height = 800;   // Set your preferred height

        //this.Icon = new WindowIcon("/Assets/appicon.ico");
    }
}