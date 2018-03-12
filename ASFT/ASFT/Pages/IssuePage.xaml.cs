﻿using ASFT.PageModels;
using FreshMvvm;
using TK.CustomMap;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ASFT.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class IssuePage : FreshBaseContentPage
    {
        public IssuePage()
        {
            InitializeComponent();
            CreateView();
            MapRelativeLayout.BindingContext = new TkMapPageModel();
        }

        public void CreateView()
        {
            var newYork = new Position(40.7142700, -74.0059700);
            var mapView = new TKCustomMap(MapSpan.FromCenterAndRadius(newYork, Distance.FromKilometers(2)));
            mapView.SetBinding(TKCustomMap.PinsProperty, "Pins");
            mapView.SetBinding(TKCustomMap.MapClickedCommandProperty, "MapClickedCommand");
            mapView.SetBinding(TKCustomMap.MapLongPressCommandProperty, "MapLongPressCommand");

            mapView.SetBinding(TKCustomMap.PinSelectedCommandProperty, "PinSelectedCommand");
            mapView.SetBinding(TKCustomMap.SelectedPinProperty, "SelectedPin");
            mapView.SetBinding(TKCustomMap.PinDragEndCommandProperty, "DragEndCommand");
            mapView.SetBinding(TKCustomMap.CirclesProperty, "Circles");
            mapView.SetBinding(TKCustomMap.CalloutClickedCommandProperty, "CalloutClickedCommand");
            mapView.SetBinding(TKCustomMap.MapRegionProperty, "MapRegion");
            mapView.SetBinding(TKCustomMap.RouteClickedCommandProperty, "RouteClickedCommand");
            mapView.SetBinding(TKCustomMap.TilesUrlOptionsProperty, "TilesUrlOptions");
            mapView.SetBinding(TKCustomMap.MapFunctionsProperty, "MapFunctions");
            mapView.IsRegionChangeAnimated = true;
            mapView.IsShowingUser = true;

            MapRelativeLayout.Children.Add(
                mapView,
                Constraint.Constant(0),
                Constraint.Constant(0)
            );
        }
    }
}