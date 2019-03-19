using System.Reactive.Concurrency;
using System.Threading;
using Karambolo.ReactiveMvvm.Internal.Platform;

namespace Karambolo.ReactiveMvvm
{
    public class WinFormsSchedulerProvider : IPlatformSchedulerProvider
    {
        public static IScheduler CaptureUIThreadScheduler()
        {
            return new SynchronizationContextScheduler(SynchronizationContext.Current);
        }

        internal WinFormsSchedulerProvider(IScheduler mainThreadScheduler)
        {
            MainThreadScheduler = mainThreadScheduler;
            TaskPoolScheduler = System.Reactive.Concurrency.TaskPoolScheduler.Default;
        }

        public IScheduler MainThreadScheduler { get; }

        public IScheduler TaskPoolScheduler { get; }
    }
}
