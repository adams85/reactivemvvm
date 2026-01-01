using System;
using System.ComponentModel;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Windows.Forms;
using GettingStarted.Messages;
using GettingStarted.ViewModels;
using Karambolo.ReactiveMvvm;

namespace GettingStarted.Views
{
    // inherit top-level views from ReactiveForm to get type-safe data/command binding and view activation capabilities;
    // this brings a further benefit: ReactiveForm implements INotifyPropertyChanging/INotifyPropertyChanged interfaces,
    // what enables you to use APIs like WhenChange to observe state changes of the view
    // (you may implement ILifetime if you want to manually control the lifetime of the view)
    public partial class MainView : ReactiveForm<MainViewModel>, ILifetime
    {
        private readonly CompositeDisposable _attachedDisposables = new CompositeDisposable();
        private readonly SerialDisposable _selectFileInteractionDisposable;

        public MainView()
        {
            InitializeComponent();

            // a) view activation is opt-in: to enable it, you need to call EnableViewActivation in the constructor,
            // then override OnViewActivated to respond to activation/deactivation events
            // (you may dispose the returned disposable if you want to cancel activation, however, it's safe to omit to dispose it otherwise)
            this.EnableViewActivation();

            // b) message bus enables you to broadcast messages to other (unknown) application components
            // (but keep in mind that usually there are better ways to share information so use message bus as a last resort!)

            // here we setup listening to LogMessage events and display them in a text box
            // (usually this would go into OnViewActivated but in this case we need to subscribe as soon as possible
            // in order not to miss early messages)
            ReactiveMvvmContext.Current.MessageBus.Listen<LogMessage>()
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe(msg => LogTextBox.AppendText(msg.Message + Environment.NewLine))
                .AttachTo(this);

            // c) you can use interactions to obtain information from the user involving the UI

            // when the view model becomes available we register our handler for the interaction it exposes
            // (it would be ok to omit to dispose the WhenChange subscription since it involves the events of this view instance only,
            // thus, the subscription doesn't prevent GC'ing the view instance)
            _selectFileInteractionDisposable = new SerialDisposable();
            _selectFileInteractionDisposable
                .AttachTo(this);

            this.WhenChange(v => v.ViewModel)
                .Subscribe(value => _selectFileInteractionDisposable.Disposable = value.GetValueOrDefault()?.SelectFileInteraction.RegisterHandler(HandleSelectFile))
                .AttachTo(this);

            void HandleSelectFile(InteractionContext<Unit, string> context)
            {
                var dialog = new OpenFileDialog();
                context.Output = dialog.ShowDialog() == DialogResult.OK ? dialog.FileName : null;
                context.IsHandled = true;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _attachedDisposables.Dispose();
                components?.Dispose();
            }
            base.Dispose(disposing);
        }

        public void AttachDisposable(IDisposable disposable)
        {
            _attachedDisposables.Add(disposable);
        }

        public void DetachDisposable(IDisposable disposable)
        {
            _attachedDisposables.Remove(disposable);
        }

        [DefaultValue(null)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ChildView ChildView
        {
            get;
            set
            {
                if (field == value)
                    return;

                if (field != null)
                {
                    ChildViewPlaceholderPanel.Controls.Remove(field);

                    // normally we'd dispose the child view but we don't want it to suppress GC finalization to see if it gets collected by the GC
                    // _childView.Dispose();

                    // WORKAROUND: layout controls tend to leak memory in WinForms, we can prevent this by calling PerformLayout on the parent control
                    // https://stackoverflow.com/questions/25181679/windows-form-memory-leak
                    ChildViewPlaceholderPanel.PerformLayout();
                }

                field = value;

                if (field != null)
                    ChildViewPlaceholderPanel.Controls.Add(field);
            }
        }

        protected override void OnViewActivated(ViewActivationLifetime activationLifetime)
        {
            // usually you set up bindings here as you want them to be active only until the view gets deactivated

            // a) binding a view model command to a view control
            // (use rbc snippet to quickly insert a command binding)

            // by default the command is triggered by Click/MouseUp events but you can specify another signal event using the eventName parameter
            this.BindCommand(ViewModel, vm => vm.ToggleChildCommand, v => v.ToggleChildViewButton)
                .AttachTo(activationLifetime);

            // command bindings can be simulated using InvokeCommand, as well
            Observable
                .FromEventPattern(h => StartInteractionButton.Click += h, h => StartInteractionButton.Click -= h)
                .Select(_ => Unit.Default)
                .InvokeCommand(this, v => v.ViewModel.StartInteractionCommand)
                .AttachTo(activationLifetime);

            // b) binding a view model property to a view property so that changes in the view model flow to the view and vice versa
            // (use rbtw snippet to quickly insert a command binding)

            // WinForms controls don't have a unified interface to notify property changes,
            // so you need to convert the corresponding event into an observable sequence and
            // use that to make the binding work in the view -> view model direction
            // (if you want to write less boilerplate code, you may consider using ReactiveUI.Events.Winforms)
            var canToggleChildViewCheckBox_CheckedChangedValues = Observable
                .FromEventPattern(h => CanToggleChildViewCheckBox.CheckedChanged += h, h => CanToggleChildViewCheckBox.CheckedChanged -= h)
                .Select(e => ((CheckBox)e.Sender).Checked);

            // it's worth noting that the value coming from the view model takes precedence (in other words, the view model "wins") when the binding is being initialized
            this.BindTwoWay(ViewModel, vm => vm.CanToggleChild, v => v.CanToggleChildViewCheckBox.Checked,
                viewValues: canToggleChildViewCheckBox_CheckedChangedValues)
                .AttachTo(activationLifetime);

            // c) binding a view model property to a view property so that changes in the view model flow to the view but not in the opposite direction
            // (use rbow snippet to quickly insert a command binding)

            this.BindOneWay(ViewModel, vm => vm.Child, v => v.ChildView, CreateChildViewForModel)
                .AttachTo(activationLifetime);

            // for the sake of demonstration we publish notifications of the activation/deactivation events to the message bus

            ReactiveMvvmContext.Current.MessageBus.Publish(new LogMessage { Message = $"{nameof(MainView)} activated" });

            Disposable.Create(() => ReactiveMvvmContext.Current.MessageBus.Publish(new LogMessage { Message = $"{nameof(MainView)} deactivated" }))
                .AttachTo(activationLifetime);
        }

        private static ChildView CreateChildViewForModel(ChildViewModel childViewModel)
        {
            if (childViewModel == null)
                return null;

            return new ChildView
            {
                ViewModel = childViewModel,
                Dock = DockStyle.Fill
            };
        }

        private void ForceGCButton_Click(object sender, EventArgs e)
        {
            // we force GC to check that child views and view models don't leak
            GC.Collect();
        }
    }
}
