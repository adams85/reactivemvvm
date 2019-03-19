using System.Reactive.Concurrency;
using Karambolo.ReactiveMvvm.ErrorHandling;
using Microsoft.Extensions.Logging;

namespace Karambolo.ReactiveMvvm
{
    public interface IReactiveMvvmContext
    {
        IScheduler MainThreadScheduler { get; }
        IScheduler TaskPoolScheduler { get; }
        ObservedErrorHandler DefaultErrorHandler { get; }
        IMessageBus MessageBus { get; }
        IViewModelFactory ViewModelFactory { get; }
        ILoggerFactory LoggerFactory { get; }
    }
}
