using ASFT.HelperMethods;
using ASFT.PageModels;
using FreshMvvm;
using Xamarin.Forms;

namespace ASFT
{
    public partial class App
    {
        public static AppIssueClient Client;

        public static int ScreenWidth = 0;
        public static int ScreenHeight = 0;
        public static float Density = 1; //DPI Scale factor

        public App()
        {
            Client = new AppIssueClient();

            Page page = FreshPageModelResolver.ResolvePageModel<IssuePageModel>();
            FreshNavigationContainer container = new FreshNavigationContainer(page);
            MainPage = container;
        }


        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}