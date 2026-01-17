using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using JamBox.Core.Models;
using JamBox.Core.ViewModels;

namespace JamBox.Core.Views.UserControls;

public partial class AlbumsListView : UserControl
{
    public AlbumsListView()
    {
        InitializeComponent();
    }

    private void AlbumsListBox_DoubleTapped(object? sender, TappedEventArgs e)
    {
        if (e.Source is not Visual visual)
        {
            return;
        }

        // Find the ListBoxItem that was double-tapped
        var listBoxItem = visual.FindAncestorOfType<ListBoxItem>();
        if (listBoxItem?.DataContext is Album album && DataContext is LibraryViewModel vm)
        {
            vm.PlayAlbumCommand.Execute(album).Subscribe();
        }
    }
}