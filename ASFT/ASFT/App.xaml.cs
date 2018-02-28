using ASFT.HelperMethods;
using ASFT.Views;
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
            // The root page of your application
            MainPage = GetMainPage();
        }

        
        void SetupIOC()
        {
            //FreshIOC.Container.Register<DataTemplate>();
        }
        public static Page GetMainPage()
        {
            return new NavigationPage(new IssueView());
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