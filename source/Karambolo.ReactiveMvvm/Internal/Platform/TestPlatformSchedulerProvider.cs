using System.Reactive.Concurrency;

namespace Karambolo.ReactiveMvvm.Internal.Platform
{
    public class TestPlatformSchedulerProvider : IPlatformSchedulerProvider
    {
        public TestPlatformSchedulerProvider(IScheduler mainThreadScheduler, IScheduler taskPoolScheduler)
        {
            MainThreadScheduler = mainThreadScheduler ?? CurrentThreadScheduler.Instance;
            TaskPoolScheduler = taskPoolScheduler ?? System.Reactive.Concurrency.TaskPoolScheduler.Default;
        }

        public IScheduler MainThreadScheduler { get; }

        public IScheduler TaskPoolScheduler { get; }
    }
}
