using System;
using Xamarin.Forms.Maps;

namespace ASFT.HelperMethods
{
    public class ExtendedMap : Map
    {
        public ExtendedMap()
        {
        }

        public ExtendedMap(MapSpan region) : base(region)
        {
        }

        public event EventHandler<TapEventArgs> Tap;
        public event EventHandler<TapEventArgs> LongTap;
        public event EventHandler<EventArgs> Ready;

        public void OnTap(Position coordinate)
        {
            OnTap(new TapEventArgs {Position = coordinate});
        }

        public void OnLongTap(Position coordinate)
        {
            OnLongTap(new TapEventArgs {Position = coordinate});
        }

        public void OnReady()
        {
            OnReady(new EventArgs());
        }


        protected virtual void OnReady(EventArgs e)
        {
            Ready?.Invoke(this, e);
        }

        protected virtual void OnTap(TapEventArgs e)
        {
            Tap?.Invoke(this, e);
        }

        protected virtual void OnLongTap(TapEventArgs e)
        {
            LongTap?.Invoke(this, e);
        }
    }

    public class TapEventArgs : EventArgs
    {
        public Position Position { get; set; }
    }
}