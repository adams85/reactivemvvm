using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Karambolo.ReactiveMvvm.Expressions;
using Karambolo.ReactiveMvvm.Properties;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Karambolo.ReactiveMvvm.ChangeNotification.Internal
{
    public class DefaultChainChangeProvider : IChainChangeProvider
    {
        private static readonly ConcurrentDictionary<(DataMemberAccessLink, bool), Unit> s_nonObservableMembers = new ConcurrentDictionary<(DataMemberAccessLink, bool), Unit>();
        private readonly Dictionary<(Type, bool), ILinkChangeProvider[]> _changeProvidersLookup;
        private readonly ILogger _logger;

        public DefaultChainChangeProvider(IOptions<ReactiveMvvmOptions> options, ILoggerFactory loggerFactory)
        {
            _changeProvidersLookup = options.Value.LinkChangeProviders
                .SelectMany(provider => provider.SupportedLinkTypes, (provider, linkType) => (provider, linkType))
                .GroupBy(item => (item.linkType, item.provider.NotifiesBeforeChange), item => item.provider)
                .ToDictionary(grouping => grouping.Key, grouping => grouping.ToArray());

            _logger = loggerFactory.CreateLogger<DefaultChainChangeProvider>();
        }

        private IObservable<ObservedChange> GetLinkChangesCore(object container, DataMemberAccessLink link, ChangeNotificationOptions options)
        {
            var beforeChange = options.HasOptions(ChangeNotificationOptions.BeforeChange);

            ILinkChangeProvider[] changeProviders = _changeProvidersLookup[(link.GetType(), beforeChange)];

            ILinkChangeProvider changeProvider;
            for (int i = 0, n = changeProviders.Length; i < n; i++)
                if ((changeProvider = changeProviders[i]).CanProvideFor(container, link))
                    return changeProvider.GetChanges(container, link);

            if (!options.HasOptions(ChangeNotificationOptions.SuppressWarnings) && !s_nonObservableMembers.ContainsKey((link, beforeChange)))
            {
                _logger.LogWarning(string.Format(Resources.ChangeNotificationNotPossible, nameof(ILinkChangeProvider)), link.InputType.Name + link, beforeChange);
                ReactiveMvvmContext.RecommendVerifyingInitialization(_logger);

                s_nonObservableMembers.TryAdd((link, beforeChange), Unit.Default);
            }

            return FallbackValueChangeProvider.Instance.GetChanges(container, link);
        }

        private IObservable<ObservedValue<object>> GetLinkChanges(in ObservedValue<object> container, DataMemberAccessLink link, ChangeNotificationOptions options)
        {
            return
                container.IsAvailable && container.Value != null ?
                GetLinkChangesCore(container.Value, link, options)
                    .Select(change => change.Link.ValueAccessor(change.Container))
                    .StartWith(link.ValueAccessor(container.Value)) :
                ReactiveMvvm.Internal.Default<ObservedValue<object>>.Observable;
        }

        public IObservable<ObservedValue<object>> GetChanges(object root, DataMemberAccessChain chain, ChangeNotificationOptions options)
        {
            var initialValue = ObservedValue.Wrap(root);

            IObservable<ObservedValue<object>> values = Observable.Return(initialValue);

            return chain.Aggregate(values, (vals, link) => vals
                .Select(value => GetLinkChanges(value, link, options))
                .Switch());
        }
    }
}
