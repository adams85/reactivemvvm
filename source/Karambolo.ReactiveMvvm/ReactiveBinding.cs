using System.Reactive.Concurrency;
using System.Threading;
using Karambolo.ReactiveMvvm.ErrorHandling;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Karambolo.ReactiveMvvm
{
    public static partial class ReactiveBinding
    {
        private static readonly ILoggerFactory s_loggerFactory = ReactiveMvvmContext.ServiceProvider.GetService<ILoggerFactory>();

        private static IScheduler GetViewThreadScheduler()
        {
            return SynchronizationContext.Current != null ?
                new SynchronizationContextScheduler(SynchronizationContext.Current, alwaysPost: false) :
                ReactiveMvvmContext.Current.MainThreadScheduler;
        }

        private static ObservedErrorHandler GetViewModelErrorHandler<TViewModel>(ObservedErrorHandler errorHandler, IBoundView<TViewModel> view)
            where TViewModel : class
        {
            return
                errorHandler ??
                (view.ViewModel as IObservedErrorSource)?.ErrorHandler ??
                ReactiveMvvmContext.Current.DefaultErrorHandler;
        }
    }
}
