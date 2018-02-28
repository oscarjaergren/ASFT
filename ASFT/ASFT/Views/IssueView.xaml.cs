using ASFT.ViewModels;
using Xamarin.Forms.Xaml;

namespace ASFT.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class IssueView
    {
        private IssueViewModel viewModel;

        public IssueView()
        {
            InitializeComponent();
            viewModel = new IssueViewModel();
            BindingContext = viewModel;
        }

        public IssueView(int issueId = 0)
        {
            InitializeComponent();
            LoadViewModel(issueId);
        }

        public void LoadViewModel(int issueId = 0)
        {
            if (viewModel != null) return;

            viewModel = new IssueViewModel();
            BindingContext = viewModel;
        }
    }
}