using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using Karambolo.Common;
using Karambolo.ReactiveMvvm.ChangeNotification.Internal;
using Karambolo.ReactiveMvvm.Expressions;
using Karambolo.ReactiveMvvm.Properties;
using Microsoft.Extensions.DependencyInjection;

namespace Karambolo.ReactiveMvvm
{
    public static partial class ChangeNotificationExtensions
    {
        static readonly IChainChangeProvider s_chainChangeProvider = ReactiveMvvmContext.ServiceProvider.GetRequiredService<IChainChangeProvider>();

        static bool HasOptions(this ChangeNotificationOptions @this, ChangeNotificationOptions options)
        {
            return (@this & options) == options;
        }

        static IObservable<ObservedValue<TValue>> GetChangesCore<TValue>(this object root, DataMemberAccessChain accessChain,
            ChangeNotificationOptions options, IEqualityComparer<TValue> comparer)
        {
            IObservable<ObservedValue<object>> values = s_chainChangeProvider.GetChanges(root, accessChain, options.HasOptions(ChangeNotificationOptions.BeforeChange));

            if (options.HasOptions(ChangeNotificationOptions.SkipInitial))
                values = values.Skip(1);

            var castValues = values.Select(value => value.Cast<TValue>());

            if (!options.HasOptions(ChangeNotificationOptions.NonDistinct))
            {
                IEqualityComparer<ObservedValue<TValue>> valueComparer;
                if (comparer != null)
                    valueComparer = DelegatedEqualityComparer.Create<ObservedValue<TValue>>(
                        (x, y) => x.IsAvailable == y.IsAvailable && (!x.IsAvailable || comparer.Equals(x.Value, y.Value)),
                        x => x.IsAvailable ? comparer.GetHashCode() : 0);
                else
                    valueComparer = EqualityComparer<ObservedValue<TValue>>.Default;

                castValues = castValues.DistinctUntilChanged(valueComparer);
            }

            return castValues;
        }

        public static IObservable<ObservedValue<TValue>> WhenChange<TValue>(this object root, DataMemberAccessChain accessChain,
            ChangeNotificationOptions options = ChangeNotificationOptions.Default, IEqualityComparer<TValue> comparer = null)
        {
            if (accessChain == null)
                throw new ArgumentNullException(nameof(accessChain));

            if (accessChain.Length > 0 &&
                (!accessChain.Head.InputType.IsAssignableFrom(root) || !typeof(TValue).IsAssignableFrom(accessChain.Tail.OutputType)))
                throw new ArgumentException(Resources.IncompatibleChain, nameof(accessChain));

            return GetChangesCore(root, accessChain, options, comparer);
        }

        public static IObservable<ObservedValue<TValue>> WhenChange<TRoot, TValue>(this TRoot root, Expression<Func<TRoot, TValue>> accessExpression,
            ChangeNotificationOptions options = ChangeNotificationOptions.Default, IEqualityComparer<TValue> comparer = null)
        {
            return GetChangesCore(root, DataMemberAccessChain.From(accessExpression), options, comparer);
        }
    }
}
