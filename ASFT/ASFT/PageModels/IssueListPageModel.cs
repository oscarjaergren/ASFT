namespace ASFT.PageModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using Acr.UserDialogs;
    using FreshMvvm;
    using IssueBase.Issue;
    using IssueManagerApiClient;
    using Xamarin.Forms;

    public class IssueListPageModel : FreshBasePageModel
    {
        public ObservableCollection<IssueModel> Issues { get; set; }

        public ICommand PullRefreshCommand
        {
            get
            {
                return new Command(OnPullRefresh, () => IsBusy == false);
            }
        }

        public ICommand OnSelectedIssueCommand
        {
            get
            {
                return onSelectIssueCommand ?? new Command<object>(OnIssueSelected);
            }
        }

        public ICommand DeleteIssueCommand
        {
            get
            {
                return deleteIssueCommand ?? new Command<object>(OnDelete);
            }
        }

        private readonly ICommand onSelectIssueCommand = null;
        private readonly ICommand deleteIssueCommand = null;

        private bool IsBusy { get; set; }

        private int LocationId { get; set; }

        private bool RefeshNeeded { get; set; }


        public IssueListPageModel()
        {
            RefeshNeeded = true;
            Issues = new ObservableCollection<IssueModel>();
        }

        public override void Init(object initData)
        {
            base.Init(initData);

            if (initData is int i)
            {
                LocationId = i;
            }
        }

        public async void OnPullRefresh()
        {
            IsBusy = true;
            await OnRefreshContent();
            IsBusy = false;
        }



        protected override async void ViewIsAppearing(object sender, EventArgs e)
        {
            if (App.Client.Initilized == false) await App.Client.Init();
            App.Client.RunInBackground(ShowIssuesFromCurrentLocation);
            await OnRefreshContent();

        }

        private async Task<bool> OnRefreshContent()
        {
            Issues.Clear();
            if (LocationId == -1 || App.Client.LoggedIn == false)
            {
                //await App.Client.ShowSelectLocation();
            }

            return await Task.Run(
                       () =>
                           {
                               try
                               {
                                   UserDialogs.Instance.ShowLoading("Loading events...", MaskType.Clear);

                                   var items = App.Client.GetIssues(LocationId);
                                   foreach (IssueModel Item in items)
                                   {
                                       Issues.Add(Item);
                                   }

                                   RefeshNeeded = false;

                                   UserDialogs.Instance.HideLoading();

                                   return true;
                               }
                               catch (UnauthorizedException)
                               {
                                   RefeshNeeded = false;
                                   UserDialogs.Instance.HideLoading();

                                   UserDialogs.Instance.Alert(
                                       "Not Unauthorized\nLogin with another account or change location");
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

        private async void ShowIssuesFromCurrentLocation()
        {
            if (LocationId == -1)
            {
                LocationId = App.Client.GetCurrentLocationId();
                if (LocationId == -1)
                {
                    await App.Client.ShowSelectLocation();
                }
            }

            if (RefeshNeeded)
            {
                await OnRefreshContent();
            }
        }

        private async void OnClickLoginLocation(object s)
        {
            await CoreMethods.PushPageModel<HomePageModel>();
        }

        private void OnClickNewIssue(object s, EventArgs e)
        {
            OnAddNewIssue();
        }

        private async void OnAddNewIssue()
        {
            if (IsBusy) return;

            await OnRefreshContent();
            await CoreMethods.PushPageModel<IssuePageModel>();
        }

        private async void OnClickSetFilter(object s, EventArgs e)
        {
            MessagingCenter.Subscribe<FilterPageModel>(
                this,
                "OnFilterPageReturned",
                async sender =>
                    {
                        MessagingCenter.Unsubscribe<FilterPageModel>(this, "OnFilterPageReturned");
                        if (App.Client.FilteringChanged)
                        {
                            await OnRefreshContent();
                        }
                    });

            App.Client.FilteringChanged = false;
            await CoreMethods.PushPageModel<FilterPageModel>();
        }

        private async void OnIssueSelected(object eventArgs)
        {
            if (!(eventArgs is IssueModel selectedItem)) return;
            if (selectedItem is IssueModel item)
            {
                await Task.Run(() => { });
                await CoreMethods.PushPageModel<IssuePageModel>(item);
            }
        }

        private async void OnDelete(object sender)
        {
            MenuItem mi = (MenuItem)sender;
            if (mi.BindingContext is IssueModel issue)
            {
                IsBusy = true;

                bool bDeleted = await Task.Run(
                                    () =>
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

                if (bDeleted == false) await CoreMethods.DisplayAlert("Failed", "Failed to remove item", "Continue");

                IsBusy = false;
            }
        }
    }
}

