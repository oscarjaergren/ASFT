using ASFT.ViewModels;

namespace ASFT.IServices
{
    public interface IView
    {
    }

    public interface IView<TViewModel> : IView where TViewModel : ViewModelBase
    {
        TViewModel ViewModel { get; set; }
    }
}