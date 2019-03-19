using Karambolo.ReactiveMvvm.Internal.Platform;

namespace Karambolo.ReactiveMvvm.ErrorHandling.Internal
{
    public class DefaultObservedErrorHandlerFactory : IDefaultObservedErrorHandlerFactory
    {
        readonly IPlatformSchedulerProvider _platformSchedulers;

        public DefaultObservedErrorHandlerFactory(IPlatformSchedulerProvider platformSchedulers)
        {
            _platformSchedulers = platformSchedulers;
        }

        public ObservedErrorHandler Create()
        {
            return new DefaultObservedErrorHandler(_platformSchedulers);
        }
    }
}
