using Avalonia.Controls;
using Avalonia.VisualTree;
using JamBox.Core.ViewModels;
using System.Reactive.Linq;

namespace JamBox.Core.Views.UserControls;

public partial class AlbumsListView : UserControl
{
    private const double ScrollThresholdPixels = 200;

    public AlbumsListView()
    {
        InitializeComponent();

        // Find the ScrollViewer inside the ListBox and subscribe to scroll events
        this.AttachedToVisualTree += (_, _) =>
        {
            var listBox = this.FindControl<ListBox>("AlbumsListBox");
            var scrollViewer = listBox?.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();

            if (scrollViewer != null)
            {
                scrollViewer.ScrollChanged += OnScrollChanged;
            }
        };
    }

    private void OnScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        if (sender is not ScrollViewer sv) return;

        // Check if we're near the bottom
        var distanceFromBottom = sv.Extent.Height - sv.Offset.Y - sv.Viewport.Height;

        if (distanceFromBottom < ScrollThresholdPixels)
        {
            if (DataContext is LibraryViewModel vm && vm.HasMoreAlbums && !vm.IsLoadingMoreAlbums)
            {
                vm.LoadMoreAlbumsCommand?.Execute().Subscribe(_ => { }, _ => { });
            }
        }
    }
}