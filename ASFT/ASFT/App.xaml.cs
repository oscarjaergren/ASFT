using ASFT.HelperMethods;
using ASFT.PageModels;
using FreshMvvm;
using Xamarin.Forms;

namespace ASFT
{
    public partial class App
    {
        public static AppIssueClient Client;

        public static Page IssuePageInstance;


        public App()
        {
            Client = new AppIssueClient();

            Page page = FreshPageModelResolver.ResolvePageModel<IssuePageModel>();
            FreshNavigationContainer IssuePageInstance = new FreshNavigationContainer(page);

            MainPage = IssuePageInstance;
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