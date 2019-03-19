using System.Reactive.Concurrency;

namespace Karambolo.ReactiveMvvm.Internal.Platform
{
    public interface IPlatformSchedulerProvider
    {
        IScheduler MainThreadScheduler { get; }
        IScheduler TaskPoolScheduler { get; }
    }
}
