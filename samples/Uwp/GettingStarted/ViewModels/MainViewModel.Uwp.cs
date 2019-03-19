using Prism.Navigation;

namespace GettingStarted.ViewModels
{
    public partial class MainViewModel : INavigationAware
    {
        public void OnNavigatedFrom(INavigationParameters parameters)
        {
            Dispose();
        }

        public void OnNavigatedTo(INavigationParameters parameters) { }

        public void OnNavigatingTo(INavigationParameters parameters) { }
    }
}
