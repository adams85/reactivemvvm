using System.Reactive.Disposables;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using GettingStarted.Messages;
using GettingStarted.ViewModels;
using Karambolo.ReactiveMvvm;

namespace GettingStarted.Views
{
    // inherit child views from ReactiveUserControl to get type-safe data/command binding and view activation capabilities
    public partial class ChildView : ReactiveUserControl<ChildViewModel>
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

        public TextBlock CurrentTimeTextBlock => this.Get<TextBlock>("CurrentTimeTextBlock");

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
