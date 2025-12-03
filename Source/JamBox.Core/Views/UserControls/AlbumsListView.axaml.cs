using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
using JamBox.Core.ViewModels;
using System.Reactive.Linq;

namespace JamBox.Core.Views.UserControls;

public partial class AlbumsListView : UserControl
{
    private const double ScrollThresholdPixels = 200;
    private ScrollViewer? _scrollViewer;

    public AlbumsListView()
    {
        InitializeComponent();

        this.AttachedToVisualTree += OnAttachedToVisualTree;
        this.DetachedFromVisualTree += OnDetachedFromVisualTree;
    }

    private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        // The ListBox with WrapPanel may not have a standard internal ScrollViewer,
        // so we need to search more thoroughly
        var listBox = this.FindControl<ListBox>("AlbumsListBox");
        
        // First try to find ScrollViewer in descendants of the ListBox
        _scrollViewer = listBox?.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
        
        // If not found in descendants, try ancestors (in case the control is inside a parent ScrollViewer)
        if (_scrollViewer == null)
        {
            _scrollViewer = this.GetVisualAncestors().OfType<ScrollViewer>().FirstOrDefault();
        }
        
        // As a last resort, search the entire visual tree from the ListBox upward
        if (_scrollViewer == null && listBox != null)
        {
            // Walk up to find any ScrollViewer that might contain us
            var current = listBox.GetVisualParent();
            while (current != null)
            {
                if (current is ScrollViewer sv)
                {
                    _scrollViewer = sv;
                    break;
                }
                current = current.GetVisualParent();
            }
        }

        if (_scrollViewer != null)
        {
            _scrollViewer.ScrollChanged += OnScrollChanged;
            System.Diagnostics.Debug.WriteLine($"AlbumsListView: Found ScrollViewer: {_scrollViewer.GetType().Name}");
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("AlbumsListView: WARNING - No ScrollViewer found!");
        }
    }

    private void OnDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (_scrollViewer != null)
        {
            _scrollViewer.ScrollChanged -= OnScrollChanged;
            _scrollViewer = null;
        }
    }

    private void OnScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        if (_scrollViewer == null) return;

        // Check if we're near the bottom
        var distanceFromBottom = _scrollViewer.Extent.Height - _scrollViewer.Offset.Y - _scrollViewer.Viewport.Height;

        System.Diagnostics.Debug.WriteLine($"Scroll: Extent={_scrollViewer.Extent.Height}, Offset={_scrollViewer.Offset.Y}, Viewport={_scrollViewer.Viewport.Height}, DistanceFromBottom={distanceFromBottom}");

        if (distanceFromBottom < ScrollThresholdPixels)
        {
            if (DataContext is LibraryViewModel vm && vm.HasMoreAlbums && !vm.IsLoadingMoreAlbums)
            {
                System.Diagnostics.Debug.WriteLine("Triggering LoadMoreAlbumsCommand");
                vm.LoadMoreAlbumsCommand?.Execute().Subscribe(_ => { }, _ => { });
            }
        }
    }
}