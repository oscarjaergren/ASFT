using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using ASFT.HelperMethods;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace ASFT.ViewModels
{
    public class MapViewModel : ExtendedMap
    {
        public class BindableMap : Map
        {
            public static readonly BindableProperty MapPinsProperty = BindableProperty.Create(
                nameof(Pins),
                typeof(ObservableCollection<Pin>),
                typeof(BindableMap),
                new ObservableCollection<Pin>(),
                propertyChanged: (b, o, n) =>
                {
                    BindableMap bindable = (BindableMap) b;
                    bindable.Pins.Clear();

                    var collection = (ObservableCollection<Pin>) n;
                    foreach (Pin item in collection)
                        bindable.Pins.Add(item);
                    collection.CollectionChanged += (sender, e) =>
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            switch (e.Action)
                            {
                                case NotifyCollectionChangedAction.Add:
                                case NotifyCollectionChangedAction.Replace:
                                case NotifyCollectionChangedAction.Remove:
                                    if (e.OldItems != null)
                                        foreach (object item in e.OldItems)
                                            bindable.Pins.Remove((Pin) item);
                                    if (e.NewItems != null)
                                        foreach (object item in e.NewItems)
                                            bindable.Pins.Add((Pin) item);
                                    break;
                                case NotifyCollectionChangedAction.Reset:
                                    bindable.Pins.Clear();
                                    break;
                            }
                        });
                    };
                });

            public static readonly BindableProperty MapPositionProperty = BindableProperty.Create(
                nameof(MapPosition),
                typeof(Position),
                typeof(BindableMap),
                new Position(0, 0),
                propertyChanged: (b, o, n) =>
                {
                    ((BindableMap) b).MoveToRegion(MapSpan.FromCenterAndRadius(
                        (Position) n,
                        Distance.FromMiles(1)));
                });

            public IList<Pin> MapPins { get; set; }

            public Position MapPosition { get; set; }
        }

        //#region Map

        //public void MapCtrlReady(object sender, EventArgs args)
        //{
        //    bMapCtrlReady = true;
        //}

        //public void OnMapLongTap(object sender, ExtendedMap.TapEventArgs args)
        //{
        //    if (AllowPinMovment == false)
        //        return;

        //    if (Issue == null)
        //        return;

        //    var pos = args.Position;

        //    // Update Issue
        //    Issue.Latitude = pos.Latitude;
        //    Issue.Longitude = pos.Longitude;
        //    Issue.Changed = true;

        //    // Update Pin
        //    map.Pins.Clear();
        //    AddPin(pos, Issue.Title, Issue.Description);
        //}

        //protected void AddPin(Xamarin.Forms.Maps.Position pos, String Title, String Desc)
        //{
        //    // MAP pin does not like it if labels are empty
        //    if (Title.Length == 0)
        //        Title = "-";

        //    if (Desc.Length == 0)
        //        Desc = "-";

        //    var pin = new Pin
        //    {
        //        Type = PinType.Place,
        //        Position = pos,
        //        Label = Title,
        //        Address = Desc
        //    };
        //    map.Pins.Add(pin);
        //}

        //void OnButtonCenter(object sender, EventArgs args)
        //{
        //    MoveToPinLocation();
        //}

        //void MoveToPinLocation()
        //{
        //    double KmDistace = 0.5;

        //    if (Issue != null)
        //    {
        //        map.MoveToRegion(MapSpan.FromCenterAndRadius(new Xamarin.Forms.Maps.Position(Issue.Latitude, Issue.Longitude), Distance.FromKilometers(KmDistace)));
        //    }
        //    else
        //        map.MoveToRegion(MapSpan.FromCenterAndRadius(new Xamarin.Forms.Maps.Position(Location.Latitude, Location.Longitude), Distance.FromKilometers(KmDistace)));

        //}

        //void OnButtonMainLocation(object sender, EventArgs args)
        //{
        //    double KmDistace = 0.5;
        //    map.MoveToRegion(MapSpan.FromCenterAndRadius(new Xamarin.Forms.Maps.Position(Location.Latitude, Location.Longitude), Distance.FromKilometers(KmDistace)));
        //}

        //void OnButtonGetLocation(object sender, EventArgs args)
        //{
        //    OnGetLocation();
        //}

        //async void OnGetLocation()
        //{
        //    if (IsGettingLocation == true)
        //        return; // already getting location

        //    try
        //    {
        //        if (Locator.IsListening == true)
        //        {
        //            await Locator.StopListeningAsync();
        //        }

        //        if (Locator.IsGeolocationAvailable == false)
        //        {
        //            lbPosText.Text = "GeoLocation is not available.";
        //            this.ForceLayout();
        //            return;
        //        }

        //        if (Locator.IsGeolocationEnabled == false)
        //        {
        //            lbPosText.Text = "GeoLocation is not enabled.";
        //            this.ForceLayout();
        //            return;
        //        }

        //        IsGettingLocation = true;

        //        IsBusy = true;
        //        slCommands.IsVisible = false;
        //        aActIndicator.IsVisible = true;
        //        aActIndicator.IsRunning = true;
        //        lbPosText.Text = "Searching for GPS location...";
        //        this.ForceLayout();

        //        TimeSpan timeSpan = TimeSpan.FromTicks(120 * 1000);
        //        var position = await Locator.GetPositionAsync(timeSpan);

        //        // Update Issue Position
        //        Issue.Latitude = position.Latitude;
        //        Issue.Longitude = position.Longitude;
        //        Issue.Changed = true;


        //        // Update Pin Postion
        //        var pos = new Xamarin.Forms.Maps.Position(Issue.Latitude, Issue.Longitude);
        //        map.Pins.Clear();
        //        AddPin(pos, Issue.Title, Issue.Description);

        //        UpdateGPSLocationText();

        //        aActIndicator.IsRunning = false;
        //        aActIndicator.IsVisible = false;
        //        IsGettingLocation = false;
        //        IsBusy = false;
        //        slCommands.IsVisible = true;
        //        this.ForceLayout();
        //        // Center map around pin
        //        MoveToPinLocation();
        //    }
        //    catch (Exception /*ex*/)
        //    {
        //        aActIndicator.IsRunning = false;
        //        aActIndicator.IsVisible = false;
        //        IsGettingLocation = false;
        //        IsBusy = false;
        //        slCommands.IsVisible = true;
        //        lbPosText.Text = "Unable to find position!";
        //        lbPosText.IsVisible = true;
        //        this.ForceLayout();
        //    }
        //}

        //void UpdateGPSLocationText()
        //{
        //    String text = String.Format("{0} x {1}", Issue.Longitude, Issue.Latitude);
        //    lbPosText.Text = text;
        //}
        //#endregion
    }
}