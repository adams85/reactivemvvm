using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using GettingStarted.Messages;
using GettingStarted.ViewModels;
using Karambolo.ReactiveMvvm;

namespace GettingStarted.Views
{
    // inherit top-level views from ReactiveWindow to get type-safe data/command binding and view activation capabilities
    // (you may implement ILifetime if you want to manually control the lifetime of the view)
    public class MainView : ReactiveWindow<MainViewModel>, ILifetime
    {
        private readonly CompositeDisposable _attachedDisposables = new CompositeDisposable();
        private readonly SerialDisposable _selectFileInteractionDisposable;

        public MainView()
        {
            this.InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif

            // a) view activation is opt-in: to enable it, you need to call EnableViewActivation in the constructor,
            // then override OnViewActivated to respond to activation/deactivation events
            // (you may dispose the returned disposable if you want to cancel activition, however, it's safe to omit to dispose it otherwise)
            this.EnableViewActivation();

            // a) view activation is opt-in: to enable it, you need to call EnableViewActivation in the constructor,
            // then override OnViewActivated to respond to activation/deactivation events
            // (you may dispose the returned disposable if you want to cancel activition, however, it's safe to omit to dispose it otherwise)
            this.EnableViewActivation();

            // b) message bus enables you to broadcast messages to other (unknown) application components
            // (but keep in mind that usually there are better ways to share information so use message bus as a last resort!)

            // here we setup listening to LogMessage events and display them in a text box
            // (usually this would go into OnViewActivated but in this case we need to subscribe as soon as possible
            // in order not to miss early messages)
            ReactiveMvvmContext.Current.MessageBus.Listen<LogMessage>()
                .ObserveOn(AvaloniaScheduler.Instance)
                .Subscribe(msg => LogTextBox.Text += msg.Message + Environment.NewLine)
                .AttachTo(this);

            // c) you can use interactions to obtain information from the user involving the UI

            // when the view model becomes available we register our handler for the interaction it exposes
            // (it would be ok to omit the disposal of the WhenChange subscription since it involves the events of this view instance only,
            // thus, the subscription doesn't prevent GC'ing the view instance)
            _selectFileInteractionDisposable = new SerialDisposable();
            _selectFileInteractionDisposable.AttachTo(this);

            this.WhenChange(v => v.ViewModel)
                .Subscribe(value => _selectFileInteractionDisposable.Disposable = value.GetValueOrDefault()?.SelectFileInteraction.RegisterHandler(HandleSelectFileAsync))
                .AttachTo(this);

            async Task HandleSelectFileAsync(InteractionContext<Unit, string> context, CancellationToken cancellationToken)
            {
                var dialog = new OpenFileDialog();
                var files = await dialog.ShowAsync(this);
                context.Output = files.FirstOrDefault();
                context.IsHandled = true;
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public void Dispose()
        {
            _attachedDisposables.Dispose();
        }

        public void AttachDisposable(IDisposable disposable)
        {
            _attachedDisposables.Add(disposable);
        }

        public void DetachDisposable(IDisposable disposable)
        {
            _attachedDisposables.Remove(disposable);
        }

        public Button ToggleChildViewButton => this.Get<Button>("ToggleChildViewButton");
        public CheckBox CanToggleChildViewCheckBox => this.Get<CheckBox>("CanToggleChildViewCheckBox");
        public Button StartInteractionButton => this.Get<Button>("StartInteractionButton");
        public TextBox LogTextBox => this.Get<TextBox>("LogTextBox");
        public ContentControl ChildViewContentControl => this.Get<ContentControl>("ChildViewContentControl");

        protected override void OnViewActivated(ViewActivationLifetime activationLifetime)
        {
            // usually you set up bindings here as you want them to be active only until the view gets deactivated in most cases

            // a) binding a view model command to a view control
            // (use rbc snippet to quickly insert a command binding)

            // by default the command is bound to the control's Command property or if there is no such property, it's triggered by Click/MouseUp events
            // but you can explicitly specify a signal event in the eventName parameter
            this.BindCommand(ViewModel, vm => vm.ToggleChildCommand, v => v.ToggleChildViewButton)
                .AttachTo(activationLifetime);

            // command bindings can be simulated using InvokeCommand, as well
            Observable
                .FromEventPattern<RoutedEventArgs>(h => StartInteractionButton.Click += h, h => StartInteractionButton.Click -= h)
                .Select(_ => Unit.Default)
                .InvokeCommand(this, v => v.ViewModel.StartInteractionCommand)
                .AttachTo(activationLifetime);

            // b) binding a view model property to a view property so that changes in the view model flow to the view and vice versa
            // (use rbtw snippet to quickly insert a command binding)

            // it's worth noting that the value coming from the view model takes precedence (in other words, the view model "wins") when the binding is being initialized
            this.BindTwoWay(ViewModel, vm => vm.CanToggleChild, v => v.CanToggleChildViewCheckBox.IsChecked,
                value => value, value => value ?? false)
                .AttachTo(activationLifetime);

            // c) binding a view model property to a view property so that changes in the view model flow to then view but changes but not in the opposite direction
            // (use rbow snippet to quickly insert a command binding)

            this.BindOneWay(ViewModel,
                vm => vm.Child,
                v => v.ChildViewContentControl.Content)
                .AttachTo(activationLifetime);

            // for the sake of demonstration we publish notifications of the activation/deactivation events to the message bus

            ReactiveMvvmContext.Current.MessageBus.Publish(new LogMessage { Message = $"{nameof(MainView)} activated" });

            Disposable.Create(() => ReactiveMvvmContext.Current.MessageBus.Publish(new LogMessage { Message = $"{nameof(MainView)} deactivated" }))
                .AttachTo(activationLifetime);
        }

        private void ForceGCButton_Click(object sender, RoutedEventArgs e)
        {
            // we force GC to see if child views and view models don't leak
            GC.Collect();
        }
    }
}
