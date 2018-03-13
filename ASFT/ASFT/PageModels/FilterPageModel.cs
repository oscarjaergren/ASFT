using System;
using System.Windows.Input;
using Acr.UserDialogs;
using ASFT.Client;
using FreshMvvm;
using Xamarin.Forms;

namespace ASFT.PageModels
{
    public class FilterPageModel : FreshBasePageModel
    {
        private const string SortAscending = "Ascending (A-Z)";
        private const string SortDescending = "Descending (Z-A)";
        public FilteringAndSorting Filtering { get; set; }
        public FilterPageModel()
        {
            Filtering = new FilteringAndSorting(App.Client.GetFilteringAndSorting());

            UpdateSortByButton();
            UpdateSortOrderButton();
        }
        private readonly ICommand onClickSortByCommand = null;
        private readonly ICommand onClickSortOrderCommand = null;

        public ICommand OnClickSortByCommand
        {
            get { return onClickSortByCommand ?? new Command(OnClickSortBy); }
        }

        public ICommand OnClickSortOrderCommand
        {
            get { return onClickSortOrderCommand ?? new Command(OnClickSortOrder); }
        }


        public string SortByText { get; set; }

        public string SortByOrderText { get; set; }

        protected void UpdateSortByButton()
        {
            SortByText = "Sort By : " + Filtering.SortBy;
        }

        protected void UpdateSortOrderButton()
        {
            if (Filtering.SortAscending)
                SortByOrderText = "Order : " + SortAscending;
            else
                SortByOrderText = "Order : " + SortDescending;
        }

        public async void OnClickSortBy()
        {
            System.Threading.CancellationToken token = new System.Threading.CancellationToken();
            string res = await UserDialogs.Instance.ActionSheetAsync("Sort Order", "Cancel", "", token, "Date", "Title", "Status", "Severity");


            if (res.Length > 0 && res != "Cancel")
                Filtering.SortBy = res;

            UpdateSortByButton();
        }
        public async void OnClickSortOrder()
        {
            System.Threading.CancellationToken token = new System.Threading.CancellationToken();
            string res = await UserDialogs.Instance.ActionSheetAsync("Sort Order", "Cancel", "", token, SortAscending, SortDescending);
            if (res.Length <= 0 || res == "Cancel") return;
            Filtering.SortAscending = res == SortAscending;

            UpdateSortOrderButton();
        }
        
        protected override void ViewIsDisappearing (object sender, EventArgs e)
        {
            if (Filtering != App.Client.GetFilteringAndSorting())
            {
                App.Client.GetFilteringAndSorting().SetFrom(Filtering);
                App.Client.SaveFiltering();
                // Bad solution
                App.Client.FilteringChanged = true;

            }
            else
            {
                App.Client.FilteringChanged = false;
            }
            MessagingCenter.Send(this, "OnFilterPageReturned");
        }

    }
}
