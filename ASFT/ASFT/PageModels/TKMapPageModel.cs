using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Plugin.Geolocator.Abstractions;
using TK.CustomMap;
using TK.CustomMap.Api;
using TK.CustomMap.Api.Google;
using TK.CustomMap.Api.OSM;
using TK.CustomMap.Interfaces;
using TK.CustomMap.Overlays;
using Xamarin.Forms;
using Position = TK.CustomMap.Position;

namespace ASFT.PageModels
{
    public class TkMapPageModel : INotifyPropertyChanged
    {

        #region Model
        private TKTileUrlOptions tileUrlOptions;

        private MapSpan mapRegion = MapSpan.FromCenterAndRadius(new Position(56.8790, 14.8059), Distance.FromKilometers(2));
        private Position mapCenter;
        private TKCustomMapPin selectedPin;
        private bool isClusteringEnabled;
        private ObservableCollection<TKCustomMapPin> pins;
        private ObservableCollection<TKRoute> routes;
        private ObservableCollection<TKCircle> circles;
        private ObservableCollection<TKPolyline> lines;
        private ObservableCollection<TKPolygon> polygons;
        private readonly Random random = new Random(1984);

        public TKTileUrlOptions TilesUrlOptions
        {
            get
            {
                return tileUrlOptions;
                //return new TKTileUrlOptions(
                //    "http://a.basemaps.cartocdn.com/dark_all/{2}/{0}/{1}.png", 256, 256, 0, 18);
                //return new TKTileUrlOptions(
                //    "http://a.tile.openstreetmap.org/{2}/{0}/{1}.png", 256, 256, 0, 18);
            }
            set
            {
                if (tileUrlOptions != value)
                {
                    tileUrlOptions = value;
                    OnPropertyChanged("TilesUrlOptions");
                }
            }
        }

        public IRendererFunctions MapFunctions { get; set; }

      

        public bool IsClusteringEnabled
        {
            get => isClusteringEnabled;
            set
            {
                isClusteringEnabled = value;
                OnPropertyChanged(nameof(IsClusteringEnabled));
            }
        }

      

        public MapSpan MapRegion
        {
            get { return mapRegion; }
            set
            {
                if (mapRegion != value)
                {
                    mapRegion = value;
                    OnPropertyChanged("MapRegion");
                }
            }
        }

        public ObservableCollection<TKCustomMapPin> Pins
        {
            get { return pins; }
            set
            {
                if (pins != value)
                {
                    pins = value;
                    OnPropertyChanged("Pins");
                }
            }
        }
        public ObservableCollection<TKRoute> Routes
        {
            get { return routes; }
            set
            {
                if (routes != value)
                {
                    routes = value;
                    OnPropertyChanged("Routes");
                }
            }
        }
        public ObservableCollection<TKCircle> Circles
        {
            get { return circles; }
            set
            {
                if (circles != value)
                {
                    circles = value;
                    OnPropertyChanged("Circles");
                }
            }
        }
        public ObservableCollection<TKPolyline> Lines
        {
            get { return lines; }
            set
            {
                if (lines != value)
                {
                    lines = value;
                    OnPropertyChanged("Lines");
                }
            }
        }
        public ObservableCollection<TKPolygon> Polygons
        {
            get { return polygons; }
            set
            {
                if (polygons != value)
                {
                    polygons = value;
                    OnPropertyChanged("Polygons");
                }
            }
        }
        public Position MapCenter
        {
            get { return mapCenter; }
            set
            {
                if (mapCenter != value)
                {
                    mapCenter = value;
                    OnPropertyChanged("MapCenter");
                }
            }
        }
        public TKCustomMapPin SelectedPin
        {
            get { return selectedPin; }
            set
            {
                if (selectedPin != value)
                {
                    selectedPin = value;
                    OnPropertyChanged("SelectedPin");
                }
            }
        }

        #endregion
        public Command<Position> MapLongPressCommand
        {
            get
            {
                return new Command<Position>(async position =>
                {
                    string action = await Application.Current.MainPage.DisplayActionSheet(
                        "Long Press",
                        "Cancel",
                        null,
                        "Add Pin",
                        "Add Circle");

                    if (action == "Add Pin")
                    {
                        TKCustomMapPin pin = new TKCustomMapPin
                        {
                            Position = position,
                            Title = string.Format("Pin {0}, {1}", position.Latitude, position.Longitude),
                            ShowCallout = true,
                            IsDraggable = true
                        };
                        pins.Add(pin);
                    }
                    else if (action == "Add Circle")
                    {
                        TKCircle circle = new TKCircle
                        {
                            Center = position,
                            Radius = 10000,
                            Color = Color.FromRgba(100, 0, 0, 80)
                        };
                        circles.Add(circle);
                    }
                });
            }
        }
        public Command<Position> MapClickedCommand
        {
            get
            {
                return new Command<Position>((positon) =>
                {
                    SelectedPin = null;

                    // Determine if a point was inside a circle
                    if ((from c in circles let distanceInMeters = c.Center.DistanceTo(positon) * 1000 where distanceInMeters <= c.Radius select c).Any())
                    {
                        Application.Current.MainPage.DisplayAlert("Circle tap", "Circle was tapped", "OK");
                    }
                });
            }
        }
        public Command<IPlaceResult> PlaceSelectedCommand
        {
            get
            {
                return new Command<IPlaceResult>(async p =>
                {
                    switch (p)
                    {
                        case GmsPlacePrediction gmsResult:
                            GmsDetailsResult details = await GmsPlace.Instance.GetDetails(gmsResult.PlaceId);
                            MapCenter = new Position(details.Item.Geometry.Location.Latitude, details.Item.Geometry.Location.Longitude);
                            return;
                        case OsmNominatimResult osmResult:
                            MapCenter = new Position(osmResult.Latitude, osmResult.Longitude);
                            return;
                    }

                    switch (Device.RuntimePlatform)
                    {
                        case Device.Android:
                        {
                            TKNativeAndroidPlaceResult prediction = (TKNativeAndroidPlaceResult)p;

                            TKPlaceDetails details = await TKNativePlacesApi.Instance.GetDetails(prediction.PlaceId);

                            MapCenter = details.Coordinate;
                            break;
                        }
                        case Device.iOS:
                        {
                            TKNativeiOSPlaceResult prediction = (TKNativeiOSPlaceResult)p;

                            MapCenter = prediction.Details.Coordinate;
                            break;
                        }
                    }
                });
            }
        }

        public Command PinSelectedCommand
        {
            get
            {
                return new Command<TKCustomMapPin>((TKCustomMapPin pin) =>
                {
                    MapRegion = MapSpan.FromCenterAndRadius(SelectedPin.Position, Distance.FromKilometers(1));
                });
            }
        }
        
        public Command ClearMapCommand
        {
            get
            {
                return new Command(() =>
                {
                    pins.Clear();
                    circles.Clear();
                    if (routes != null)
                        routes.Clear();
                });
            }
        }

        private IGeolocator geolocator;
        
        private readonly ICommand initMapCommand = null;

        public string MapText { get; set; }


        public ICommand InitMapCommand
        {
            get { return initMapCommand ?? new Command(  (async () => await GetCurrentLocationAsync())); }
        }
      
        private void AddPin(Position position)
        {
            TKCustomMapPin pin = new TKCustomMapPin
            {
                Position = position,
                Title = string.Empty,
                ShowCallout = false
            };
            Pins.Clear();
            Pins.Add(pin);
        }

        private async Task GetCurrentLocationAsync()
        {
            try
            {
                MapText = "Searching for GPS location...";

                TimeSpan timeSpan = TimeSpan.FromTicks(120 * 1000);
                Plugin.Geolocator.Abstractions.Position position = await geolocator.GetPositionAsync(timeSpan);
                if (position != null)
                {
                    MapCenter = new Position(position.Latitude, position.Longitude);
                    MapRegion = MapSpan.FromCenterAndRadius(MapCenter, Distance.FromKilometers(2));
                    Position x = new Position(position.Latitude, position.Longitude);
                    mapRegion = MapSpan.FromCenterAndRadius(new Position(x.Latitude, x.Longitude), Distance.FromKilometers(2));
                    AddPin(x);

                    // Update Issue Position
                    //Issue.Latitude = position.Latitude;
                    //Issue.Longitude = position.Longitude;
                    //Changed = true;

                    // Update Pin Postion
                    UpdateGpsLocationText(x);

                }
            }
            catch (Exception /*ex*/)
            {
                MapText = "Unable to find position!";
            }
          
        }
       
        private void UpdateGpsLocationText(Position position)
        {
            string text = string.Format("{0} x {1}", position.Longitude, position.Latitude);
            MapText = text;
        }



        public TkMapPageModel()
        {
            mapCenter = new Position(40.7142700, -74.0059700);
            pins = new ObservableCollection<TKCustomMapPin>();
            GetLocation();
            circles = new ObservableCollection<TKCircle>();

        }

        private async void GetLocation()
        {
           await GetCurrentLocationAsync();
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
