using System;
using System.Windows.Input;
using Acr.UserDialogs;
using ASFT.Client;
using Xamarin.Forms;

namespace ASFT.ViewModels
{
    public partial class FilterViewModel : FilteringAndSorting
    {

        private readonly string sortAscending = "Ascending (A-Z)";
        private readonly string sortDescending = "Descending (Z-A)";
        public FilteringAndSorting Filtering { get; set; }
        public FilterViewModel()
        {
            Filtering = new FilteringAndSorting(App.Client.GetFilteringAndSorting());
            BindingContext = Filtering;

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
                SortByOrderText = "Order : " + sortAscending;
            else
                SortByOrderText = "Order : " + sortDescending;
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
            string res = await UserDialogs.Instance.ActionSheetAsync("Sort Order", "Cancel", "", token, sortAscending, sortDescending);
            if (res.Length <= 0 || res == "Cancel") return;
            Filtering.SortAscending = res == sortAscending;

            UpdateSortOrderButton();
        }

        protected override void OnDisappearing()
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
            base.OnDisappearing();
        }

    }
}
