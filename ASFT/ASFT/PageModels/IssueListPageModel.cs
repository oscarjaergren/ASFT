using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
        public ObservableCollection<IssueModel> Issues { get; set; }
        private int LocationId { get; set; }
        private bool RefeshNeeded { get; set; }
        public bool IsBusy { get; set; }

        public IssueListPageModel()
        {
            //Title = "Events";
            RefeshNeeded = true;
            Issues = new ObservableCollection<IssueModel>();
        }

        public override void Init(object initData)
        {
            if (initData is int i)
            {
                LocationId = i;
                initData = LocationId;

            }
            base.Init(initData);

        }

       
        public async void OnPullRefresh()
        {
            IsBusy = true;
            await OnRefreshContent();
            IsBusy = false;
        }

        private async Task<bool> OnRefreshContent()
        {
            Issues.Clear();
            if (LocationId == -1 || App.Client.LoggedIn == false)
            {
                return false;
            }

            return await Task.Run(() =>
            {
                try
                {
                    UserDialogs.Instance.ShowLoading("Loading events...", maskType: MaskType.Clear);

                    var items = App.Client.GetIssues(LocationId);
                    foreach (IssueModel Item in items)
                    {
                        Issues.Add(Item);
                    }
                    RefeshNeeded = false;

                    UserDialogs.Instance.HideLoading();

                    return true;
                }
                catch (IssueManagerApiClient.UnauthorizedException)
                {
                    RefeshNeeded = false;
                    UserDialogs.Instance.HideLoading();

                    UserDialogs.Instance.Alert("Not Unauthorized\nLogin with another account or change location");
                    return false;
                }
                catch (Exception)
                {
                    RefeshNeeded = false;
                    UserDialogs.Instance.Alert("Failed. Unknown error while getting issues..");
                    return false;
                }
            });
        }
        protected override async void ViewIsAppearing(object sender, EventArgs e)
        {
            if (App.Client.Initilized == false) await App.Client.Init();
            await OnRefreshContent();
            App.Client.RunInBackground(ShowIssuesFromCurrentLocation);
        }

        protected async void ShowIssuesFromCurrentLocation()
        {
            if (LocationId == -1)
            {
                LocationId = App.Client.GetCurrentLocationId();
                if (LocationId == -1)
                {
                    await ShowSelectLocation();
                }
            }
            if (RefeshNeeded)
            {
                await OnRefreshContent();
            }
        }

        private async void OnClickLoginLocation(object s, EventArgs e)
        {
            MessagingCenter.Subscribe<HomePageModel>(this, "LocationChanged", (sender) =>
            {
                MessagingCenter.Unsubscribe<HomePageModel>(this, "LocationChanged");
                LocationId = -1;
                RefeshNeeded = true;
                ShowIssuesFromCurrentLocation();
            });

            await CoreMethods.PushPageModel<HomePageModel>();

        }

        private void OnClickNewIssue(object s, EventArgs e)
        {
            OnAddNewIssue();
        }

        private async void OnAddNewIssue()
        {
            if (IsBusy)
                return;

            await OnRefreshContent();
            await CoreMethods.PushPageModel<IssuePageModel>();
        }

        private async void OnClickSetFilter(object s, EventArgs e)
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

        public ICommand PullRefreshCommand
        {
            get
            {
                return new Command(OnPullRefresh, () => IsBusy == false);
            }
        }


        public ICommand OnSelectedIssueCommand
        {
            get { return onSelectIssueCommand ?? new Command<object>(OnEventSelected); }


        }

        private readonly ICommand onSelectIssueCommand = null;


        public async void OnEventSelected(object eventArgs)
        {
            if (!(eventArgs is IssueModel selectedItem)) return;
            if (selectedItem is IssueModel item)
            {
                await Task.Run(() =>
                {

                });
                await CoreMethods.PushPageModel<IssuePageModel>(item);
            }

        }

        public async void OnDelete(object sender, EventArgs e)
        {
            MenuItem mi = ((MenuItem)sender);
            if (mi.BindingContext is IssueModel issue)
            {
                IsBusy = true;

                bool bDeleted = await Task.Run(() =>
                {
                    try
                    {
                        App.Client.DeleteIssue(issue.ServerId);
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
        #region Map

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
                var buttons = new string[locations.Count];
                for (int n = 0; n < locations.Count; ++n)
                {
                    buttons[n] = locations[n].Id + " - " + locations[n].Name;
                }

                string res = await CoreMethods.DisplayActionSheet("Pick Location", "Cancel", "", buttons);
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
                    foreach (LocationModel loc in locations)
                    {
                        if (loc.Id == id)
                        {
                            LocationId = id;
                            App.Client.SetCurrentLocation(loc);
                            RefeshNeeded = true;
                            return true;
                        }
                    }
                }
                return false;
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception);
                return false;
            }
        }
        #endregion

    }
}

