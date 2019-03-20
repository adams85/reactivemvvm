﻿using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using GettingStarted.Messages;
using Karambolo.ReactiveMvvm;
using Microsoft.Extensions.Logging;

namespace GettingStarted.ViewModels
{
    // inherit view models from ReactiveObject to get change notification and error handling capabilities,
    // and implement IActivableViewModel if you want to respond to view activation events of the related view
    public partial class MainViewModel : ReactiveObject, IActivableViewModel
    {
        private readonly IReactiveMvvmContext _context;
        private readonly ILogger _logger;

        // it's recommended to register and resolve the view models from the IoC container (see ReactiveMvvmBuilderExtensions.RegisterViewModels),
        // so you can inject your dependencies through constructor parameters
        public MainViewModel(IReactiveMvvmContext context) : base(context.DefaultErrorHandler)
        {
            _context = context;
            _logger = context.LoggerFactory.CreateLogger<MainViewModel>();

            // you can turn change notifications of a property (or property chain) into observable sequences using WhenChange
            var canToggleChild = this.WhenChange(vm => vm.CanToggleChild).Select(value => value.GetValueOrDefault());

            // you can create a command from sync/async delegates (see ReactiveCommand.ToCommand overloads),
            // and control its CanExecute behavior by an observable sequence
            ToggleChildCommand = new Action(
                () =>
                {
                    _logger.LogInformation($"{nameof(ToggleChildCommand)} is executed at {{TIME}}", DateTime.UtcNow);
                    Child = Child == null ? Child = context.ViewModelFactory.CreateViewModel<ChildViewModel>() : null;
                })
                .ToCommand(this, canToggleChild);

            StartInteractionCommand = new Func<CancellationToken, Task>(
                async ct =>
                {
                    _logger.LogInformation($"{nameof(StartInteractionCommand)} is executed at {{TIME}}", DateTime.UtcNow);
                    var message = await SelectFileInteraction.HandleAsync(Unit.Default, ct).ConfigureAwait(false);
                    message = message != null ? $"You selected the file '{message}'" : $"File selection was cancelled.";
                    _context.MessageBus.Publish(new LogMessage { Message = message });
                })
                .ToCommand(this);

            SelectFileInteraction = new Interaction<Unit, string>();
        }

        // use rmcd snippet to quickly insert notifying properties 
        public ReactiveCommand<Unit, Unit> ToggleChildCommand { get; }

        public ReactiveCommand<Unit, Unit> StartInteractionCommand { get; }

        // use rint snippet to quickly insert notifying properties 
        public Interaction<Unit, string> SelectFileInteraction { get; }

        // use rprop snippet to quickly insert properties which notifies of changes
        private bool _canToggleChild;
        public bool CanToggleChild
        {
            get => _canToggleChild;
            set => Change(ref _canToggleChild, value);
        }

        private ChildViewModel _child;
        public ChildViewModel Child
        {
            get => _child;
            set
            {
                if (_child == value)
                    return;

                if (_child != null)
                {
                    // child is about to discarded so dispose of it right now
                    _child.DetachFrom(this);
                    _child.Dispose();
                }

                // change child and notify about the change
                Change(ref _child, value);

                if (_child != null)
                    // make sure that child is disposed together with this instance
                    _child.AttachTo(this);
            }
        }

        public void OnViewActivated(ViewActivationLifetime activationLifetime)
        {
            _context.MessageBus.Publish(new LogMessage { Message = $"{nameof(MainViewModel)} activated" });

            Disposable.Create(() => _context.MessageBus.Publish(new LogMessage { Message = $"{nameof(MainViewModel)} deactivated" }))
                .AttachTo(activationLifetime);
        }
    }
}
