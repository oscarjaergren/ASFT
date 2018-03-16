using Acr.UserDialogs;
using Android.App;
using Android.Content.PM;
using Android.OS;
using TK.CustomMap.Droid;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

namespace ASFT.Droid
{
    [Activity(Label = "ASFT", Icon = "@drawable/icon", MainLauncher = false, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : FormsApplicationActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            UserDialogs.Init(this);
            TKGoogleMaps.Init(this, bundle); 
            Xamarin.FormsMaps.Init(this, bundle);
            Forms.Init(this, bundle);
            LoadApplication(new App());
        }
    }
}