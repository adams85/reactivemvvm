using System.Reactive.Concurrency;
using Karambolo.ReactiveMvvm.Internal.Platform;

namespace Karambolo.ReactiveMvvm
{
    public class UnoSchedulerProvider : IPlatformSchedulerProvider
    {
        public static IScheduler CaptureUIThreadScheduler()
        {
#if TARGETS_WINUI
            return DispatcherQueueScheduler.Current;
#else
            return UnoDispatcherScheduler.Current;
#endif
        }

        internal UnoSchedulerProvider(IScheduler mainThreadScheduler)
        {
            MainThreadScheduler = mainThreadScheduler;
            TaskPoolScheduler = System.Reactive.Concurrency.TaskPoolScheduler.Default;
        }

        public IScheduler MainThreadScheduler { get; }

        public IScheduler TaskPoolScheduler { get; }
    }
}
