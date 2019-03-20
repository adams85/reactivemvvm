using System.Windows.Media.Imaging;
using Karambolo.ReactiveMvvm;
using NugetSearchDemo.ViewModels;

namespace NugetSearchDemo.Views
{
    // The class derives off ReactiveUserControl which contains the ViewModel property.
    public partial class NugetDetailsView : ReactiveUserControl<NugetDetailsViewModel>
    {
        public NugetDetailsView()
        {
            InitializeComponent();

            this.EnableViewActivation();
        }

        protected override void OnViewActivated(ViewActivationLifetime activationLifetime)
        {
            // Our 4th parameter we convert from Url into a BitmapImage. 
            // This is an easy way of doing value conversion using ReactiveMvvm binding.
            this.BindOneWay(ViewModel,
                viewModel => viewModel.IconUrl,
                view => view.iconImage.Source,
                url => url == null ? null : new BitmapImage(url))
                .AttachTo(activationLifetime);

            this.BindOneWay(ViewModel,
                viewModel => viewModel.Title,
                view => view.titleRun.Text)
                .AttachTo(activationLifetime);

            this.BindOneWay(ViewModel,
                viewModel => viewModel.Description,
                view => view.descriptionRun.Text)
                .AttachTo(activationLifetime);

            this.BindCommand(ViewModel,
                viewModel => viewModel.OpenPage,
                view => view.openButton)
                .AttachTo(activationLifetime);
        }
    }
}
