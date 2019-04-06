using System;
using System.Diagnostics;
using System.Reactive.Concurrency;
using Karambolo.ReactiveMvvm.Internal.Platform;

namespace Karambolo.ReactiveMvvm.ErrorHandling.Internal
{
    public class DefaultObservedErrorHandler : ObservedErrorHandler
    {
        private readonly IPlatformSchedulerProvider _platformSchedulers;

        public DefaultObservedErrorHandler(IPlatformSchedulerProvider platformSchedulers)
        {
            _platformSchedulers = platformSchedulers;
        }

        public override void Handle(Exception exception)
        {
            if (Debugger.IsAttached)
                Debugger.Break();

            _platformSchedulers.MainThreadScheduler.Schedule(() => base.Handle(exception));
        }
    }
}
