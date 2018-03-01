using ASFT.ViewModels;
using Xamarin.Forms.Xaml;

namespace ASFT.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class IssueView 
    {

        public IssueView()
        {
            InitializeComponent();
            //viewModel = new IssueViewModel();
            //BindingContext = viewModel;
        }

        public IssueView(int issueId = 0)
        {
            InitializeComponent();
            LoadViewModel(issueId);
        }

        public void LoadViewModel(int issueId = 0)
        {

        }
    }
}