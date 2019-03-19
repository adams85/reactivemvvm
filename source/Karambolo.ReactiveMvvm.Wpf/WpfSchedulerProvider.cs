using System.Reactive.Concurrency;
using Karambolo.ReactiveMvvm.Internal.Platform;

namespace Karambolo.ReactiveMvvm
{
    public class WpfSchedulerProvider : IPlatformSchedulerProvider
    {
        public static IScheduler CaptureUIThreadScheduler()
        {
            return DispatcherScheduler.Current;
        }

        internal WpfSchedulerProvider(IScheduler mainThreadScheduler)
        {
            MainThreadScheduler = mainThreadScheduler;
            TaskPoolScheduler = System.Reactive.Concurrency.TaskPoolScheduler.Default;
        }

        public IScheduler MainThreadScheduler { get; }

        public IScheduler TaskPoolScheduler { get; }
    }
}
