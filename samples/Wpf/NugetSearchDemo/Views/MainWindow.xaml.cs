using Karambolo.ReactiveMvvm;
using NugetSearchDemo.ViewModels;

namespace NugetSearchDemo.Views
{
    public partial class MainWindow : ReactiveWindow<AppViewModel>
    {
        public MainWindow()
        {
            InitializeComponent();

            this.EnableViewActivation();
        }

        protected override void OnViewActivated(ViewActivationLifetime activationLifetime)
        {
            // We create our bindings here. These are the code behind bindings which allow 
            // type safety. The bindings will only become active when the Window is being shown.
            // We register our subscriptions in activationLifetime, this will cause 
            // the binding subscriptions to become inactive when the Window is closed.
            // The activationLifetime is an ILifetime to which we can "attach" other disposables 
            // to dispose them all in one go when the lifetime ends (that is, gets disposed).
            // We use the AttachTo extension method which simply adds 
            // the subscription disposable to the lifetime object.

            // Notice we don't have to provide a converter, on WPF a global converter is
            // registered which knows how to convert a boolean into visibility.
            this.BindOneWay(ViewModel,
                viewModel => viewModel.IsAvailable,
                view => view.searchResultsListBox.Visibility)
                .AttachTo(activationLifetime);

            this.BindOneWay(ViewModel,
                viewModel => viewModel.SearchResults,
                view => view.searchResultsListBox.ItemsSource)
                .AttachTo(activationLifetime);

            this.BindTwoWay(ViewModel,
                viewModel => viewModel.SearchTerm,
                view => view.searchTextBox.Text)
                .AttachTo(activationLifetime);
        }
    }
}
