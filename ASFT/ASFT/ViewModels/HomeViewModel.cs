using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Acr.UserDialogs;
using ASFT.Views;
using IssueBase.Location;
using IssueManagerApiClient;
using Xamarin.Forms;

namespace ASFT.ViewModels
{
    public class HomeViewModel : ContentPage, INotifyPropertyChanged
    {
        public new INavigation Navigation;
        private readonly int nCurrentLocation = -1;
        public HomeViewModel()
        {
            nCurrentLocation = App.Client.GetCurrentLocationId();
        }
        private void UpdateUi()
        {
            string currentLocation = App.Client.GetCurrentLocationName();
            string currentUsername = App.Client.GetCurrentUsername();

            currentUsername = App.Client.LoggedIn == true ? String.Format("Logged in as : {0}", currentUsername) : String.Format("Logged in as : (Not logged in)");

            currentLocation = String.Format("Current Location : {0}", currentLocation);

            loginLogOutText = App.Client.LoggedIn == true ? "Log out" : "Log in";

        }

        private readonly ICommand loginLogOutCommand = null;

        public ICommand LoginLogOutCommand
        {
            get { return loginLogOutCommand ?? new Command(LoginLogOut); }
        }

        private readonly ICommand viewIssuesCommand = null;

        public ICommand ViewIssuesCommand
        {
            get { return viewIssuesCommand ?? new Command(ViewIssues); }
        }

        private readonly ICommand selectLocationCommand = null;

        public ICommand SelectLocationCommand
        {
            get { return selectLocationCommand ?? new Command(SelectLocation); }
        }


        private string loginLogOutText;

        public string LoginLogOutText
        {
            get { return loginLogOutText; }
            set
            {
                loginLogOutText = value;
                RaisePropertyChanged("Username");
            }
        }



        private string currentUserName;

        public string CurrentUsername
        {
            get { return currentUserName; }
            set
            {
                currentUserName = value;
                RaisePropertyChanged("Username");
            }
        }
        private string currentLocation;

        public string CurrentLocation
        {
            get { return currentLocation; }
            set
            {
                currentLocation = value;
                RaisePropertyChanged("Username");
            }
        }

        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public new event PropertyChangedEventHandler PropertyChanged;

        //protected override async void OnAppearing()
        //{
        //    if (App.Client.Initilized == false)
        //    {
        //        await App.Client.Init();
        //    }

        //    UpdateUi();
        //}

        //protected override void OnDisappearing()
        //{
        //    if (nCurrentLocation != App.Client.GetCurrentLocationId())
        //    {
        //        MessagingCenter.Send<HomePage>(this, "LocationChanged");
        //    }
        //    base.OnDisappearing();
        //}

        private void LoginLogOut()
        {
            if (App.Client.LoggedIn)
            {
                App.Client.Logout();
                UpdateUi();
            }
            else
            {
                OnShowLoginUpdate();
            }
        }

        private async void OnShowLoginUpdate()
        {
            MessagingCenter.Subscribe<LoginView>(this, "OnLoginPageClosed", (sender) =>
            {
                MessagingCenter.Unsubscribe<LoginView>(this, "OnLoginPageClosed");
                UpdateUi();
            });

            await this.Navigation.PushModalAsync(new LoginView());
        }

        private List<LocationModel> GetLocations()
        {
            try
            {
                return App.Client.GetLocations();
            }
            catch (ServerNotFoundException)
            {
                DisplayAlert("Failed", "Failed to connect to server", "Continue");
                return null;
            }
            catch (NotLoggedInException /*ex*/)
            {
                DisplayAlert("Failed", "Failed. Not logged in", "Continue");
                return null;
            }
            catch (Exception /*ex*/)
            {
                DisplayAlert("Failed", "Unexpected error", "Quit");
                throw;
            }
        }

        private bool enbleButton;

        public bool EnableButton
        {
            get { return enbleButton; }
            set
            {
                enbleButton = value;
                RaisePropertyChanged("EnableButton");
            }
        }



        private void EnableButtons(bool button)
        {
            button = true;
            enbleButton = true;
        }

        public async void OnCmdGetLocations()
        {
            EnableButtons(false);

            List<LocationModel> locations = null;
            await Task.Run(() =>
            {
                UserDialogs.Instance.ShowLoading("Loading Locations...", maskType: MaskType.None);
                locations = GetLocations();
                UserDialogs.Instance.HideLoading();
            });

            if (locations != null)
                OnSelectLocation(locations);

            EnableButtons(true);
        }

        private async void ViewIssues()
        {
            try
            {
                // URLY..  HATE ASYNC !
                if (App.Client.LoggedIn == false)
                {
                    await this.Navigation.PushModalAsync(new LoginView());
                }

                if (App.Client.LoggedIn == false)
                    return;

                OnCmdGetLocations();

                if (App.Client.GetCurrentLocationId() == -1)
                    return;

                IssueView page = new IssueView(App.Client.GetCurrentLocationId());
                await Navigation.PushModalAsync(page, true);
            }
            catch (Exception)
            {
                int x = 0;
                x++;
            }

        }
        private void SelectLocation()
        {
            //IssueCarouselPage page = new IssueCarouselPage(new UIIssueVM(true));
            //Navigation.PushModalAsync(page, true);
        }


        private async void OnSelectLocation(IReadOnlyList<LocationModel> locations)
        {
            try
            {
                string[] buttons = new string[locations.Count];
                for (int n = 0; n < locations.Count; ++n)
                {
                    buttons[n] = locations[n].Id + " - " + locations[n].Name;
                }

                string res = await this.DisplayActionSheet("Pick Location", "Cancel", "", buttons);
                if (res == "Cancel")
                    return;

                string locationName = "";
                int id = Convert.ToInt32(res.Substring(0, 2));
                int pos = res.IndexOf('-');
                if (pos > 0)
                    locationName = res.Substring(pos + 1);

                locationName = locationName.Trim();

                if (id <= 0) return;
                foreach (var loc in locations)
                {
                    if (loc.Id == id)
                    {
                        App.Client.SetCurrentLocation(loc);
                        UpdateUi();
                        break;
                    }
                }
            }
            catch (Exception)
            {
                int x = 0;
                x++;
            }
        }
    }
    }
