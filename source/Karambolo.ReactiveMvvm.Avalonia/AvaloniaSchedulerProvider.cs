using System.Reactive.Concurrency;
using Avalonia.Threading;
using Karambolo.ReactiveMvvm.Internal.Platform;

namespace Karambolo.ReactiveMvvm
{
    public class AvaloniaSchedulerProvider : IPlatformSchedulerProvider
    {
        public static readonly IScheduler DefaultUIThreadScheduler = AvaloniaScheduler.Instance;

        internal AvaloniaSchedulerProvider(IScheduler mainThreadScheduler)
        {
            MainThreadScheduler = mainThreadScheduler;
            TaskPoolScheduler = System.Reactive.Concurrency.TaskPoolScheduler.Default;
        }

        public IScheduler MainThreadScheduler { get; }

        public IScheduler TaskPoolScheduler { get; }
    }
}
