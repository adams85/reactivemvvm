using System.Reactive.Disposables;
using GettingStarted.Helpers;
using GettingStarted.Messages;
using GettingStarted.ViewModels;
using Karambolo.ReactiveMvvm;

namespace GettingStarted.Views
{
    // WORKAROUND: https://stackoverflow.com/questions/33550495/self-referencing-generic-type-constraint-and-xaml-in-uwp-application
    public class ChildViewBase : ReactiveUserControl<ChildViewModel> { }

    // inherit child views from ReactiveUserControl to get type-safe data/command binding and view activation capabilities
    public sealed partial class ChildView : ChildViewBase
    {
        public ChildView()
        {
            InitializeComponent();

            this.EnableViewActivation();
        }

        ~ChildView()
        {
            ReactiveMvvmContext.Current.MessageBus.Publish(new LogMessage { Message = $"{nameof(ChildView)} finalized" });
        }

        protected override void OnViewActivated(ViewActivationLifetime activationLifetime)
        {
            // UWP controls don't like when one tries to set their Text properties to null,
            // so we need a BindingConverter which deals with unavailable values to keep them happy
            this.BindOneWay(ViewModel, vm => vm.CurrentTime, v => v.CurrentTimeTextBlock.Text, new SafeTimeConverter())
                .AttachTo(activationLifetime);

            ReactiveMvvmContext.Current.MessageBus.Publish(new LogMessage { Message = $"{nameof(ChildView)} activated" });

            Disposable.Create(() => ReactiveMvvmContext.Current.MessageBus.Publish(new LogMessage { Message = $"{nameof(ChildView)} deactivated" }))
                .AttachTo(activationLifetime);
        }
    }
}
