using Avalonia.Controls;
using JamBox.Core.ViewModels;

namespace JamBox.Core.Views.UserControls;

public partial class TracksListView : UserControl
{
    public TracksListView()
    {
        InitializeComponent();
    }

    private void ListBox_DoubleTapped(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        if (sender is ListBox listBox && listBox.SelectedItem != null)
        {
            if (DataContext is LibraryViewModel viewModel)
            {
                viewModel.PlayCommand?.Execute();
            }
        }
    }
}