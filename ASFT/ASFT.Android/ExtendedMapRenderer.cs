using System;
using Android.Content;
using Android.Gms.Maps;
using Android.Runtime;
using ASFT.HelperMethods;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Maps.Android;
using Xamarin.Forms.Platform.Android;

namespace ASFT.Droid
{

    public class ExtendedMapRenderer : MapRenderer, IOnMapReadyCallback
    {


        private GoogleMap _map;
        public ExtendedMapRenderer(Context context) : base(context)
        {
            AutoPackage = false;
        }



        private void InvokeOnMapReadyBaseClassHack(GoogleMap googleMap)
        {
            System.Reflection.MethodInfo onMapReadyMethodInfo = null;

            Type baseType = typeof(MapRenderer);
            foreach (var currentMethod in baseType.GetMethods(System.Reflection.BindingFlags.NonPublic |
                                                             System.Reflection.BindingFlags.Instance |
                                                              System.Reflection.BindingFlags.DeclaredOnly))
            {

                if (currentMethod.IsFinal && currentMethod.IsPrivate)
                {
                    if (string.Equals(currentMethod.Name, "OnMapReady", StringComparison.Ordinal))
                    {
                        onMapReadyMethodInfo = currentMethod;

                        break;
                    }

                    if (currentMethod.Name.EndsWith(".OnMapReady", StringComparison.Ordinal))
                    {
                        onMapReadyMethodInfo = currentMethod;

                        break;
                    }
                }
            }

            if (onMapReadyMethodInfo != null)
            {
                onMapReadyMethodInfo.Invoke(this, new[] { googleMap });
            }
        }


        void IOnMapReadyCallback.OnMapReady(GoogleMap googleMap)
        {
            InvokeOnMapReadyBaseClassHack(googleMap);
            _map = googleMap;
            if (_map != null)
            {
                _map = googleMap;
                this.NativeMap = googleMap;
                _map.MapClick += GoogleMap_MapClick;
                _map.MapLongClick += GoogleMap_MapLongClick;

                ((ExtendedMap)Element).OnReady();
            }
        }
        protected override void OnElementChanged(ElementChangedEventArgs<Map> e)
        {
            if (_map != null)
                _map.MapClick -= GoogleMap_MapClick;
            base.OnElementChanged(e);
            if (Control != null)
                ((MapView)Control).GetMapAsync(this);
        }

        private void GoogleMap_MapClick(object sender, GoogleMap.MapClickEventArgs e)
        {
            ((ExtendedMap)Element).OnTap(new Position(e.Point.Latitude, e.Point.Longitude));
        }


        private void GoogleMap_MapLongClick(object sender, GoogleMap.MapLongClickEventArgs e)
        {
            ((ExtendedMap)Element).OnLongTap(new Position(e.Point.Latitude, e.Point.Longitude));
        }
    }
}
