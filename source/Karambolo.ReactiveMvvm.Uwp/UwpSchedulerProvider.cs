using System.Reactive.Concurrency;
using Karambolo.ReactiveMvvm.Internal.Platform;

namespace Karambolo.ReactiveMvvm
{
    public class UwpSchedulerProvider : IPlatformSchedulerProvider
    {
        public static IScheduler CaptureUIThreadScheduler()
        {
            return CoreDispatcherScheduler.Current;
        }

        internal UwpSchedulerProvider(IScheduler mainThreadScheduler)
        {
            MainThreadScheduler = mainThreadScheduler;
            TaskPoolScheduler = System.Reactive.Concurrency.TaskPoolScheduler.Default;
        }

        public IScheduler MainThreadScheduler { get; }

        public IScheduler TaskPoolScheduler { get; }
    }
}
