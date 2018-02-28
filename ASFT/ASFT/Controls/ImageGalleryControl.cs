using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Xamarin.Forms;

namespace ASFT.Controls
{
    public class ImageGalleryControl : ScrollView
    {
        public static BindableProperty ItemsSourceProperty = BindableProperty.Create(nameof(ItemsSource), typeof(IList),
            typeof(ImageGalleryControl), BindingMode.TwoWay,
            propertyChanging: (bindableObject, oldValue, newValue) =>
            {
                ((ImageGalleryControl) bindableObject).ItemsSourceChanging();
            },
            propertyChanged: (bindableObject, oldValue, newValue) =>
            {
                ((ImageGalleryControl) bindableObject).ItemsSourceChanged(newValue);
            }
        );

        public static readonly BindableProperty SelectedItemProperty =
            BindableProperty.Create(nameof(SelectedItem), typeof(ImageGalleryControl), typeof(object), null,
                BindingMode.TwoWay,
                propertyChanged: (bindable, oldValue, newValue) =>
                {
                    ((ImageGalleryControl) bindable).UpdateSelectedIndex();
                });

        public static readonly BindableProperty SelectedIndexProperty =
            BindableProperty.Create(nameof(SelectedIndex), typeof(int), null, BindingMode.TwoWay,
                propertyChanged: (bindable, oldValue, newValue) =>
                {
                    ((ImageGalleryControl) bindable).UpdateSelectedItem();
                });

        private readonly StackLayout imageStack;

        public ImageGalleryControl()
        {
            Orientation = ScrollOrientation.Horizontal;

            imageStack = new StackLayout
            {
                Orientation = StackOrientation.Horizontal
            };

            Content = imageStack;
        }

        public new IList<View> Children
        {
            get { return imageStack.Children; }
        }

        public IList ItemsSource
        {
            get { return (IList) GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public DataTemplate ItemTemplate { get; set; }

        public object SelectedItem
        {
            get { return GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        public int SelectedIndex
        {
            get { return (int) GetValue(SelectedIndexProperty); }
            set { SetValue(SelectedIndexProperty, value); }
        }


        private void ItemsSourceChanging()
        {
            if (ItemsSource == null)
                return;
        }

        private void ItemsSourceChanged(object newValue)
        {
            if (ItemsSource == null)
                return;

            if (newValue is INotifyCollectionChanged notifyCollection)
                notifyCollection.CollectionChanged += (sender, args) =>
                {
                    if (args.NewItems != null)
                    {
                        if (args.NewItems.Count > 0)
                            foreach (object newItem in args.NewItems)
                            {
                                View view = (View) ItemTemplate.CreateContent();
                                if (view is BindableObject bindableObject)
                                    bindableObject.BindingContext = newItem;
                                imageStack.Children.Add(view);
                            }
                    }
                    else
                    {
                        imageStack.Children.Clear();
                        foreach (object item in ItemsSource)
                        {
                            View view = (View) ItemTemplate.CreateContent();
                            if (view is BindableObject bindableObject)
                                bindableObject.BindingContext = item;
                            imageStack.Children.Add(view);
                        }
                    }

                    if (args.OldItems != null)
                    {
                        // not supported
                    }
                };
        }

        private void UpdateSelectedIndex()
        {
            if (SelectedItem == BindingContext)
                return;

            SelectedIndex = Children
                .Select(c => c.BindingContext)
                .ToList()
                .IndexOf(SelectedItem);
        }

        private void UpdateSelectedItem()
        {
            SelectedItem = SelectedIndex > -1 ? Children[SelectedIndex].BindingContext : null;
        }
    }
}