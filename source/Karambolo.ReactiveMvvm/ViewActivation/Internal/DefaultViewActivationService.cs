using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Runtime.CompilerServices;
using System.Threading;
using Karambolo.ReactiveMvvm.Properties;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Karambolo.ReactiveMvvm.ViewActivation.Internal
{
    public class DefaultViewActivationService : IViewActivationService
    {
        static readonly ConditionalWeakTable<IActivableView, ViewActivationTargetActivator> s_viewActivators = new ConditionalWeakTable<IActivableView, ViewActivationTargetActivator>();
        static readonly ConditionalWeakTable<IActivableViewModel, ViewActivationTargetActivator> s_viewModelActivators = new ConditionalWeakTable<IActivableViewModel, ViewActivationTargetActivator>();

        static readonly ConcurrentDictionary<Type, Unit> s_inactivableViewTypes = new ConcurrentDictionary<Type, Unit>();

        readonly IViewActivationEventProvider[] _eventProviders;
        readonly ILogger _logger;

        public DefaultViewActivationService(IOptions<ReactiveMvvmOptions> options, ILoggerFactory loggerFactory)
        {
            _eventProviders = options.Value.ViewActivationEventProviders.ToArray();
            _logger = loggerFactory?.CreateLogger<DefaultViewActivationService>() ?? (ILogger)NullLogger.Instance;
        }

        protected virtual IDisposable SetupViewModelActivation(IObservable<bool> activationEvents, IBoundView view)
        {
            var viewModelChangedSubscription = new SerialDisposable();
            var viewModelActivationSerial = new SerialDisposable();

            return new CompositeDisposable(
                activationEvents.Subscribe(activated =>
                {
                    if (activated)
                    {
                        viewModelChangedSubscription.Disposable = view.WhenChange(v => v.ViewModel)
                            .Subscribe(value =>
                            {
                                // we need to make sure to respect ordering so that the cleanup happens before we activate again
                                viewModelActivationSerial.Disposable = null;

                                if (value.IsAvailable && value.Value is IActivableViewModel viewModel)
                                {
                                    var viewModelActivator = s_viewModelActivators.GetValue(viewModel, vm => new ViewActivationTargetActivator(vm));
                                    viewModelActivationSerial.Disposable = viewModelActivator.Activate();
                                }
                            });
                    }
                    else
                    {
                        viewModelChangedSubscription.Disposable = null;
                        viewModelActivationSerial.Disposable = null;
                    }
                }),
                viewModelChangedSubscription,
                viewModelActivationSerial);
        }

        protected virtual IDisposable SetupViewActivation(IObservable<bool> activationEvents, IActivableView view)
        {
            var viewActivationSerial = new SerialDisposable();

            return new CompositeDisposable(
                activationEvents.Subscribe(activated =>
                {
                    viewActivationSerial.Disposable = null;

                    if (activated)
                    {
                        var viewActivator = s_viewActivators.GetValue(view, v => new ViewActivationTargetActivator(v));
                        viewActivationSerial.Disposable = viewActivator.Activate();
                    }
                }),
                viewActivationSerial);
        }

        protected virtual IObservable<bool> GetActivationEvents(IActivableView view)
        {
            // TODO: cache by type?

            IViewActivationEventProvider eventProvider;
            for (int i = 0, n = _eventProviders.Length; i < n; i++)
                if ((eventProvider = _eventProviders[i]).CanProvideFor(view))
                    return eventProvider.GetActivationEvents(view);

            var viewType = view.GetType();
            if (!s_inactivableViewTypes.ContainsKey(viewType))
            {
                _logger.LogWarning(string.Format(Resources.ViewActivationNotPossible, nameof(IViewActivationEventProvider), viewType));
                ReactiveMvvmContext.RecommendVerifyingInitialization(_logger);

                s_inactivableViewTypes.TryAdd(viewType, Unit.Default);
            }

            return FallbackViewActivationEventProvider.Instance.GetActivationEvents(view);
        }

        public IDisposable EnableViewActivation(IActivableView view)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            var activationEvents = GetActivationEvents(view);

            var viewModelActivation =
                view is IBoundView boundView ?
                SetupViewModelActivation(activationEvents, boundView) :
                null;

            var viewActivation = SetupViewActivation(activationEvents, view);

            return viewModelActivation != null ? new CompositeDisposable(viewModelActivation, viewActivation) : viewActivation;
        }
    }
}
