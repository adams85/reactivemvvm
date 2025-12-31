using System.Reactive.Concurrency;
using Karambolo.ReactiveMvvm.Internal.Platform;

namespace Karambolo.ReactiveMvvm
{
    public class MauiSchedulerProvider : IPlatformSchedulerProvider
    {
        public static IScheduler CaptureUIThreadScheduler()
        {
#if TARGETS_WINUI
            return DispatcherQueueScheduler.Current;
#else
            return DispatcherScheduler.Current;
#endif
        }

        internal MauiSchedulerProvider(IScheduler mainThreadScheduler)
        {
            MainThreadScheduler = mainThreadScheduler;
            TaskPoolScheduler = System.Reactive.Concurrency.TaskPoolScheduler.Default;
        }

        public IScheduler MainThreadScheduler { get; }

        public IScheduler TaskPoolScheduler { get; }
    }
}
