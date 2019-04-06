using System.Reactive.Disposables;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using GettingStarted.Messages;
using GettingStarted.ViewModels;
using Karambolo.ReactiveMvvm;

namespace GettingStarted.Views
{
    // WORKAROUND: Avalonia doesn't support generic types in XAML at the moment, so we resort to the intermediate base class trick
    public class ChildViewBase : ReactiveUserControl<ChildViewModel> { }

    // inherit child views from ReactiveWindow to get type-safe data/command binding and view activation capabilities
    public class ChildView : ChildViewBase
    {
        public ChildView()
        {
            InitializeComponent();

            this.EnableViewActivation();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        ~ChildView()
        {
            ReactiveMvvmContext.Current.MessageBus.Publish(new LogMessage { Message = $"{nameof(ChildView)} finalized" });
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
