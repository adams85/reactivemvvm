using System.Diagnostics;
using System.Reactive.Concurrency;
using Karambolo.ReactiveMvvm.Properties;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Karambolo.ReactiveMvvm.Internal.Platform
{
    public class FallbackPlatformSchedulerProvider : IPlatformSchedulerProvider
    {
        public FallbackPlatformSchedulerProvider(ILoggerFactory loggerFactory)
        {
            var message = string.Format(Resources.PlatformNotRegistered, nameof(ReactiveMvvmContext), nameof(ReactiveMvvmContext.Initialize));

            Trace.WriteLine(message);

            ILogger logger = loggerFactory?.CreateLogger<FallbackPlatformSchedulerProvider>() ?? (ILogger)NullLogger.Instance;
            logger.LogWarning(message);
        }

        public IScheduler MainThreadScheduler => null;

        public IScheduler TaskPoolScheduler => null;
    }
}
