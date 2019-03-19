using System.Reactive.Disposables;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using GettingStarted.Messages;
using GettingStarted.ViewModels;
using Karambolo.ReactiveMvvm;

namespace GettingStarted.Views
{
    // inherit child views from ReactiveWindow to get type-safe data/command binding and view activation capabilities

    // WORKAROUND: Avalonia doesn't support generic types in XAML at the moment, so we need to resort to the intermediate base class trick
    public class ChildViewBase : ReactiveUserControl<ChildViewModel> { }

    public class ChildView : ChildViewBase
    {
        public ChildView()
        {
            this.EnableViewActivation();

            // for some technical reason InitializeComponent must come after EnableViewActivation at the moment (this limitation will likely be removed soon)
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public TextBlock CurrentTimeTextBlock => this.FindControl<TextBlock>("CurrentTimeTextBlock");


        protected override void OnViewActivated(ViewActivationLifetime activationLifetime)
        {
            this.BindOneWay(ViewModel, vm => vm.CurrentTime, v => v.CurrentTimeTextBlock.Text,
                value => value.ToLocalTime().TimeOfDay.ToString("hh':'mm':'ss"))
                .AttachTo(activationLifetime);

            ReactiveMvvmContext.Current.MessageBus.Publish(new LogMessage { Message = $"{nameof(ChildView)} activated" });

            Disposable.Create(() => ReactiveMvvmContext.Current.MessageBus.Publish(new LogMessage { Message = $"{nameof(ChildView)} deactivated" }))
                .AttachTo(activationLifetime);
        }
    }
}
