using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reflection;
using Karambolo.ReactiveMvvm.Expressions;
using Karambolo.ReactiveMvvm.Properties;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Karambolo.ReactiveMvvm.ChangeNotification.Internal
{
    public class DefaultChainChangeProvider : IChainChangeProvider
    {
        static readonly ConcurrentDictionary<(DataMemberAccessLink, bool), Unit> s_nonObservableMembers = new ConcurrentDictionary<(DataMemberAccessLink, bool), Unit>();

        readonly Dictionary<(Type, bool), ILinkChangeProvider[]> _changeProvidersLookup;
        readonly ILogger _logger;

        public DefaultChainChangeProvider(IOptions<ReactiveMvvmOptions> options, ILoggerFactory loggerFactory)
        {
            _changeProvidersLookup = options.Value.LinkChangeProviders
                .SelectMany(provider => provider.SupportedLinkTypes, (provider, linkType) => (provider, linkType))
                .GroupBy(item => (item.linkType, item.provider.NotifiesBeforeChange), item => item.provider)
                .ToDictionary(grouping => grouping.Key, grouping => grouping.ToArray());

            _logger = loggerFactory.CreateLogger<DefaultChainChangeProvider>();
        }

        IObservable<ObservedChange> GetLinkChangesCore(object container, DataMemberAccessLink link, bool beforeChange)
        {
            ILinkChangeProvider[] changeProviders = _changeProvidersLookup[(link.GetType(), beforeChange)];

            ILinkChangeProvider changeProvider;
            for (int i = 0, n = changeProviders.Length; i < n; i++)
                if ((changeProvider = changeProviders[i]).CanProvideFor(container, link))
                    return changeProvider.GetChanges(container, link);

            if (!s_nonObservableMembers.ContainsKey((link, beforeChange)))
            {
                _logger.LogWarning(string.Format(Resources.ChangeNotificationNotPossible, nameof(ILinkChangeProvider)), link.InputType.Name + link, beforeChange);
                ReactiveMvvmContext.RecommendCheckingInitialization(_logger);

                s_nonObservableMembers.TryAdd((link, beforeChange), Unit.Default);
            }

            return FallbackValueChangeProvider.Instance.GetChanges(container, link);
        }

        IObservable<ObservedValue<object>> GetLinkChanges(in ObservedValue<object> container, DataMemberAccessLink link, bool beforeChange)
        {
            return
                container.IsAvailable && container.Value != null ?
                GetLinkChangesCore(container.Value, link, beforeChange)
                    .Select(change => change.Link.ValueAccessor(change.Container))
                    .StartWith(link.ValueAccessor(container.Value)) :
                ReactiveMvvm.Internal.Default<ObservedValue<object>>.Observable;
        }

        public IObservable<ObservedValue<object>> GetChanges(object root, DataMemberAccessChain chain, bool beforeChange)
        {
            var initialValue = ObservedValue.Wrap(root);

            IObservable<ObservedValue<object>> values = Observable.Return(initialValue);

            return chain.Aggregate(values, (vals, link) => vals
                .Select(value => GetLinkChanges(value, link, beforeChange))
                .Switch());
        }
    }
}
