using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Acr.UserDialogs;
using FreshMvvm;
using IssueBase.Issue;
using IssueBase.Location;
using Xamarin.Forms;

namespace ASFT.PageModels
{
    public class IssueListPageModel : FreshBasePageModel
    {
        private bool FirstTimeShown { get; set; }
        public ObservableCollection<IssueModel> Issues { get; set; }
        int LocationID { get; set; }
        bool BRefeshNeeded { get; set; }
        public bool IsBusy { get; set; }

        public IssueListPageModel(int LocationID)
        {
            FirstTimeShown = true;
            this.LocationID = LocationID;
            //Title = "Events";
            BRefeshNeeded = true;
            Issues = new ObservableCollection<IssueModel>();




            var home = new ToolbarItem
            {
                Text = "Add New",
                Icon = "Add.png",
                Order = ToolbarItemOrder.Primary,
                Priority = 0,
            };
            home.Clicked += this.OnClickNewIssue;
            this.ToolbarItems.Add(home);

            //var map = new ToolbarItem
            //{
            //    Text = "Map",
            //    Icon = "map.png",
            //    Order = ToolbarItemOrder.Primary,
            //    Priority = 0,
            //};
            //map.Clicked += this.OnClickShowMap;
            //this.ToolbarItems.Add(map);


            var loginLocation = new ToolbarItem
            {
                Text = "Login and Location",
                Icon = "menu.png",
                Order = ToolbarItemOrder.Secondary,
                Priority = 0,
            };
            loginLocation.Clicked += this.OnClickLoginLocation;
            this.ToolbarItems.Add(loginLocation);

            var filter = new ToolbarItem
            {
                Text = "Filter and Sorting",
                Icon = "filter.png",
                Order = ToolbarItemOrder.Secondary,
                Priority = 1,
            };
            filter.Clicked += this.OnClickSetFilter;
            this.ToolbarItems.Add(filter);
        }

      

        public ICommand PullRefreshCommand
        {
            get
            {
                return new Command(OnPullRefresh, () => IsBusy == false);
            }
        }
        public async void OnPullRefresh()
        {
            IsBusy = true;
            await OnRefreshContent(false);
            IsBusy = false;
        }

        async Task<bool> OnRefreshContent(bool bShowLoading = true)
        {
            Issues.Clear();
            if (LocationID == -1 || App.Client.LoggedIn == false)
            {
                return false;
            }

            return await Task.Run(() =>
            {
                try
                {
                    if (bShowLoading)
                        UserDialogs.Instance.ShowLoading("Loading events...", maskType: MaskType.Clear);

                    var Items = App.Client.GetIssues(LocationID);
                    foreach (var Item in Items)
                    {
                        Issues.Add(Item);
                    }
                    BRefeshNeeded = false;

                    if (bShowLoading)
                        UserDialogs.Instance.HideLoading();

                    return true;
                }
                catch (IssueManagerApiClient.UnauthorizedException)
                {
                    BRefeshNeeded = false;
                    if (bShowLoading)
                        UserDialogs.Instance.HideLoading();

                    UserDialogs.Instance.Alert("Not Unauthorized\nLogin with another account or change location");
                    return false;
                }
                catch (Exception)
                {
                    BRefeshNeeded = false;
                    UserDialogs.Instance.Alert("Failed. Unknown error while getting issues..");
                    return false;
                }
            });
        }
        protected override async void ViewIsAppearing(object sender, EventArgs e)
        {
            if (App.Client.Initilized == false) await App.Client.Init();
            await OnRefreshContent();
            ShowIssuesFromCurrentLocation();
        }

        protected async void ShowIssuesFromCurrentLocation()
        {
            if (LocationID == -1)
            {
                // Check 
                LocationID = App.Client.GetCurrentLocationId();
                if (LocationID == -1)
                {
                    bool bChanged = await ShowSelectLocation();
                }
            }
            if (BRefeshNeeded)
            {
                await OnRefreshContent();
            }
        }

        async void OnClickLoginLocation(object s, EventArgs e)
        {
            MessagingCenter.Subscribe<HomePageModel>(this, "LocationChanged", (sender) =>
            {
                MessagingCenter.Unsubscribe<HomePageModel>(this, "LocationChanged");
                LocationID = -1;
                BRefeshNeeded = true;
                ShowIssuesFromCurrentLocation();
            });

            await CoreMethods.PushPageModel<HomePageModel>();

        }

        void OnClickNewIssue(object s, EventArgs e)
        {
            OnAddNewIssue();
        }

        async void OnAddNewIssue()
        {
            if (IsBusy)
                return;

            await OnRefreshContent();
            await CoreMethods.PushPageModel<IssuePageModel>();
        }

        async void OnClickSetFilter(object s, EventArgs e)
        {
            MessagingCenter.Subscribe<FilterPageModel>(this, "OnFilterPageReturned", async (sender) =>
            {
                MessagingCenter.Unsubscribe<FilterPageModel>(this, "OnFilterPageReturned");
                if (App.Client.FilteringChanged == true)
                {
                    await OnRefreshContent();
                }
            });

            App.Client.FilteringChanged = false;
            await CoreMethods.PushPageModel<FilterPageModel>();

        }
        //async void OnClickShowMap(object s, EventArgs e)
        //{
        //    IssueMap page = new IssueMap(Issues.ToList(), App.Client.GetCurrentGeoLocation());
        //    await CoreMethods.PushPageModel(page, true);
        //}

        
        //public ICommand OnSelectedIssueCommand
        //{
        //    //get
        //    //{
        //    //    OnEventSelected(object o, ItemTappedEventArgs e);
        //    //}
        //}

        public async void OnEventSelected(object o, ItemTappedEventArgs e)
        {
            if (IsBusy)
                return;


            if (e.Item is IssueModel item)
            {
                MessagingCenter.Subscribe<IssuePageModel>(this, "refresh", async (sender) =>
                {
                    MessagingCenter.Unsubscribe<IssueListPageModel>(this, "refresh");
                    await OnRefreshContent();
                });

                await Task.Run(() =>
                {
                    UserDialogs.Instance.ShowLoading("Loading event...", maskType: MaskType.Clear);

                    // Does lost of stuff, So show loading message

                    UserDialogs.Instance.HideLoading();
                });

                if (e.Item != null)
                    await CoreMethods.PushPageModel<IssuePageModel>(item);
            }

        }

        public async void OnDelete(object sender, EventArgs e)
        {
            var mi = ((MenuItem)sender);
            if (mi.BindingContext is IssueModel issue)
            {
                IsBusy = true;

                bool bDeleted = await Task.Run(() =>
                {
                    try
                    {
                        App.Client.DeleteIssue(issue.Id);
                        return Issues.Remove(issue);
                    }
                    catch (Exception /*ex*/)
                    {
                        return false;
                    }
                });

                if (bDeleted == false)
                    await CoreMethods.DisplayAlert("Failed", "Failed to remove item", "Continue");

                IsBusy = false;

            }
        }



        private async Task<bool> ShowSelectLocation()
        {
            var locations = GetLocations();
            if (locations != null)
                return await OnShowSelectLocationSheet(locations);

            return false;
        }
        private List<LocationModel> GetLocations()
        {
            List<LocationModel> locations = null;
            try
            {
                IsBusy = true;
                locations = App.Client.GetLocations();
                IsBusy = false;
            }
            catch (IssueManagerApiClient.ServerNotFoundException)
            {
                IsBusy = false;
                CoreMethods.DisplayAlert("Failed", "Failed to connect to server", "Continue");
            }
            catch (IssueManagerApiClient.NotLoggedInException /*ex*/)
            {
                IsBusy = false;
                MessagingCenter.Subscribe<LoginPageModel>(this, "OnLoggedIn", (sender) =>
                {
                    MessagingCenter.Unsubscribe<LoginPageModel>(this, "OnLoggedIn");
                });
            }
            catch (Exception /*ex*/)
            {
                IsBusy = false;
                CoreMethods.DisplayAlert("Failed", "Unknown error", "Quit");
                throw;
            }
            return locations;

        }


        private async Task<bool> OnShowSelectLocationSheet(List<LocationModel> locations)
        {
            try
            {
                string[] buttons = new string[locations.Count];
                for (int n = 0; n < locations.Count; ++n)
                {
                    buttons[n] = locations[n].Id + " - " + locations[n].Name;
                }

                var res = await CoreMethods.DisplayActionSheet("Pick Location", "Cancel", "", buttons);
                if (res == "Cancel")
                    return false;

                string locationName = "";
                int id = Convert.ToInt32(res.Substring(0, 2));
                int pos = res.IndexOf('-');
                if (pos > 0)
                    locationName = res.Substring(pos + 1);

                locationName = locationName.Trim();

                if (id > 0)
                {
                    foreach (var loc in locations)
                    {
                        if (loc.Id == id)
                        {
                            LocationID = id;
                            App.Client.SetCurrentLocation(loc);
                            BRefeshNeeded = true;
                            return true;
                        }
                    }
                }
                return false;
            }
            catch (Exception)
            {
                int x = 0;
                x++;
                return false;
            }
        }
    }
}

