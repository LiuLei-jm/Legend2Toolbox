
namespace Legend2Toolbox.WpfClient.Behaviors;

public static class AutoScrollBehavior
{
    public static bool GetEnable(DependencyObject obj)
    {
        return (bool)obj.GetValue(EnableProperty);
    }

    public static void SetEnable(DependencyObject obj, bool value)
    {
        obj.SetValue(EnableProperty, value);
    }

    public static readonly DependencyProperty EnableProperty =
        DependencyProperty.RegisterAttached("Enable", typeof(bool), typeof(AutoScrollBehavior), new PropertyMetadata(false, OnEnableChanged));

    private static readonly DependencyProperty CollectionChangedHandlerProperty =
        DependencyProperty.RegisterAttached("CollectionChangedHandler", typeof(NotifyCollectionChangedEventHandler), typeof(AutoScrollBehavior));
    private static readonly DependencyProperty ScrollChangedHandlerProperty =
        DependencyProperty.RegisterAttached("ScrollChangedHandler", typeof(ScrollChangedEventHandler), typeof(AutoScrollBehavior));
    private static readonly DependencyProperty IsAutoScrollingProperty =
        DependencyProperty.RegisterAttached("IsAutoScrolling", typeof(bool), typeof(AutoScrollBehavior), new PropertyMetadata(true));
    private static void OnEnableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ItemsControl itemsControl)
        {
            if ((bool)e.NewValue)
            {
                AttachBehavior(itemsControl);
                itemsControl.Unloaded += ItemsControl_Unloaded;
            }
            else
            {
                itemsControl.Unloaded -= ItemsControl_Unloaded;
                DetachBehavior(itemsControl);
            }
        }
    }

    private static void DetachBehavior(ItemsControl itemsControl)
    {
        if (itemsControl.ItemsSource is INotifyCollectionChanged collection)
        {
            var handler = (NotifyCollectionChangedEventHandler)itemsControl.GetValue(CollectionChangedHandlerProperty);
            if (handler != null)
            {
                collection.CollectionChanged -= handler;
                itemsControl.ClearValue(CollectionChangedHandlerProperty);
            }
        }
        var scrollViewer = FindScrollViewer(itemsControl);
        if (scrollViewer != null)
        {
            var handler = (ScrollChangedEventHandler)itemsControl.GetValue(ScrollChangedHandlerProperty);
            if (handler != null)
            {
                scrollViewer.ScrollChanged -= handler;
                itemsControl.ClearValue(ScrollChangedHandlerProperty);
            }
        }
    }


    private static void ItemsControl_Unloaded(object sender, RoutedEventArgs e)
    {
        if (sender is not ItemsControl itemsControl) return;
        DetachBehavior(itemsControl);
    }

    private static void AttachBehavior(ItemsControl itemsControl)
    {
        var scrollViewer = FindScrollViewer(itemsControl);
        if (scrollViewer == null)
        {
            itemsControl.Dispatcher.InvokeAsync(() =>
            {
                var sv = FindScrollViewer(itemsControl);
                if (sv != null)
                {
                    InternalAttachBehavior(itemsControl, sv);
                }
            }, DispatcherPriority.Loaded);
        }
        else
            InternalAttachBehavior(itemsControl, scrollViewer);
    }

    private static void InternalAttachBehavior(ItemsControl itemsControl, ScrollViewer scrollViewer)
    {
        if (itemsControl.GetValue(CollectionChangedHandlerProperty) != null) return;
        ScrollChangedEventHandler scrollChangedHandler = (s, args) =>
        {
            bool atButtom = scrollViewer.VerticalOffset >= scrollViewer.ScrollableHeight - 2;
            itemsControl.SetValue(IsAutoScrollingProperty, atButtom);
        };
        scrollViewer.ScrollChanged += scrollChangedHandler;
        itemsControl.SetValue(ScrollChangedHandlerProperty, scrollChangedHandler);
        if (itemsControl.ItemsSource is INotifyCollectionChanged collection)
        {
            NotifyCollectionChangedEventHandler collectionChangedHandler = (s, args) =>
            {
                var currentAutoScrollState = (bool)itemsControl.GetValue(IsAutoScrollingProperty);
                if (!currentAutoScrollState) return;
                itemsControl.Dispatcher.InvokeAsync(() =>
                {
                    scrollViewer.ScrollToEnd();
                }, DispatcherPriority.ApplicationIdle);
            };
            collection.CollectionChanged += collectionChangedHandler;
            itemsControl.SetValue(CollectionChangedHandlerProperty, collectionChangedHandler);
        }
        itemsControl.Dispatcher.InvokeAsync(() =>
        {
            scrollViewer.ScrollToEnd();
        }, DispatcherPriority.ApplicationIdle
            );
    }

    private static ScrollViewer FindScrollViewer(DependencyObject d)
    {
        if (d is ScrollViewer sv) return sv;
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(d); i++)
        {
            var child = VisualTreeHelper.GetChild(d, i);
            var result = FindScrollViewer(child);
            if (result != null) return result;
        }
        return null!;
    }
}
