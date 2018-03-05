using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using ASFT.HelperMethods;
using Xamarin.Forms;

namespace ASFT.Controls
{
    public class ImageGalleryControl : ScrollView
    {
        private readonly StackLayout imageStack;
        public ImageGalleryControl()
        {
            this.Orientation = ScrollOrientation.Horizontal;

            imageStack = new StackLayout
            {
                Orientation = StackOrientation.Horizontal
            };

            this.Content = imageStack;
        }

        public IList<View> Children
        {
            get
            {
                return imageStack.Children;
            }
        }

        public static readonly BindableProperty ItemsSourceProperty =
            BindableProperty.Create<ImageGalleryControl, IList>(
                view => view.ItemsSource,
                default(IList),
                BindingMode.TwoWay,
                propertyChanging: (bindableObject, oldValue, newValue) => {
                    ((ImageGalleryControl)bindableObject).ItemsSourceChanging();
                },
                propertyChanged: (bindableObject, oldValue, newValue) => {
                    ((ImageGalleryControl)bindableObject).ItemsSourceChanged(bindableObject, oldValue, newValue);
                }
            );

        public IList ItemsSource
        {
            get
            {
                return (IList)GetValue(ItemsSourceProperty);
            }
            set
            {
                SetValue(ItemsSourceProperty, value);
            }
        }

        #region Events
        public delegate void LoadingImagesHandler(object sender, LoadingEventArgs e);
        public event LoadingImagesHandler LoadingImages;
        #endregion

        void ItemsSourceChanging()
        {
            if (ItemsSource == null)
                return;
        }

        void CreateNewItem(IList newItem)
        {
            var view = (View)ItemTemplate.CreateContent();
            var bindableObject = view as BindableObject;
            if (bindableObject != null)
                bindableObject.BindingContext = newItem;
            imageStack.Children.Add(view);
        }

        void ItemsSourceChanged(BindableObject bindable, IList oldValue, IList newValue)
        {
            if (ItemsSource == null)
                return;

            var notifyCollection = newValue as INotifyCollectionChanged;
            if (notifyCollection != null)
            {
                notifyCollection.CollectionChanged += (sender, args) => {
                    if (args.NewItems != null)
                    {
                        if (args.NewItems.Count > 0)
                        {
                            foreach (var newItem in args.NewItems)
                            {
                                var view = (View)ItemTemplate.CreateContent();
                                var bindableObject = view as BindableObject;
                                if (bindableObject != null)
                                    bindableObject.BindingContext = newItem;
                                imageStack.Children.Add(view);
                            }
                        }
                    }
                    else
                    {
                        imageStack.Children.Clear();
                        foreach (var Item in ItemsSource)
                        {
                            var view = (View)ItemTemplate.CreateContent();
                            var bindableObject = view as BindableObject;
                            if (bindableObject != null)
                                bindableObject.BindingContext = Item;
                            imageStack.Children.Add(view);
                        }
                    }
                    if (args.OldItems != null)
                    {
                        // not supported
                    }
                };
            }
        }

        public DataTemplate ItemTemplate
        {
            get;
            set;
        }

        public static readonly BindableProperty SelectedItemProperty =
            BindableProperty.Create<ImageGalleryControl, object>(
                view => view.SelectedItem,
                null,
                BindingMode.TwoWay,
                propertyChanged: (bindable, oldValue, newValue) => {
                    ((ImageGalleryControl)bindable).UpdateSelectedIndex();
                }
            );

        public object SelectedItem
        {
            get
            {
                return GetValue(SelectedItemProperty);
            }
            set
            {
                SetValue(SelectedItemProperty, value);
            }
        }

        void UpdateSelectedIndex()
        {
            if (SelectedItem == BindingContext)
                return;

            SelectedIndex = Children
                .Select(c => c.BindingContext)
                .ToList()
                .IndexOf(SelectedItem);

        }

        public static readonly BindableProperty SelectedIndexProperty =
            BindableProperty.Create<ImageGalleryControl, int>(
                carousel => carousel.SelectedIndex,
                0,
                BindingMode.TwoWay,
                propertyChanged: (bindable, oldValue, newValue) => {
                    ((ImageGalleryControl)bindable).UpdateSelectedItem();
                }
            );

        public int SelectedIndex
        {
            get
            {
                return (int)GetValue(SelectedIndexProperty);
            }
            set
            {
                SetValue(SelectedIndexProperty, value);
            }
        }

        void UpdateSelectedItem()
        {
            SelectedItem = SelectedIndex > -1 ? Children[SelectedIndex].BindingContext : null;
        }
    }
}
