using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using GettingStarted.Messages;
using Karambolo.ReactiveMvvm;

namespace GettingStarted.ViewModels
{
    public class ChildViewModel : ReactiveObject, IActivableViewModel
    {
        private readonly IReactiveMvvmContext _context;

        public ChildViewModel(IReactiveMvvmContext context) : base(context.DefaultErrorHandler)
        {
            _context = context;
        }

        ~ChildViewModel()
        {
            ReactiveMvvmContext.Current.MessageBus.Publish(new LogMessage { Message = $"{nameof(ChildViewModel)} finalized" });
        }

        // use rpropo snippet to quickly insert computed properties backed by an observable sequence
        private ReactiveProperty<DateTimeOffset> _currentTime;
        public DateTimeOffset CurrentTime => _currentTime?.Value ?? default;

        public void OnViewActivated(ViewActivationLifetime activationLifetime)
        {
            // you can create computed properties from an observable sequence
            // (usually such properties are created in the constructor using the ReactiveProperty.ToProperty overloads,
            // but in this specific case we don't want to unnecessarily use system resources (namely, a timer) when the related view is not active)
            _currentTime = Observable
                .Interval(TimeSpan.FromMilliseconds(100))
                .StartWith(0)
                .Timestamp()
                .Select(value => value.Timestamp)
                .ToPropertyUnattached(this, vm => vm.CurrentTime, errorHandler: ErrorHandler)
                .AttachTo(activationLifetime);

            _context.MessageBus.Publish(new LogMessage { Message = $"{nameof(ChildViewModel)} activated" });

            Disposable.Create(() => _context.MessageBus.Publish(new LogMessage { Message = $"{nameof(ChildViewModel)} deactivated" }))
                .AttachTo(activationLifetime);
        }
    }
}
