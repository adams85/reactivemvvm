using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Linq;
using Karambolo.Common;
using Karambolo.ReactiveMvvm.Expressions;
using Karambolo.ReactiveMvvm.Internal;

namespace Karambolo.ReactiveMvvm.ChangeNotification.Internal
{
    public class NccIndexerLinkChangedProvider : ILinkChangeProvider
    {
        public bool NotifiesBeforeChange => false;

        public IEnumerable<Type> SupportedLinkTypes => EnumerableUtils.Return(typeof(IndexerAccessLink));

        public bool CanProvideFor(object container, DataMemberAccessLink link)
        {
            var indexerLink = (IndexerAccessLink)link;
            Type[] argTypes = indexerLink.ArgumentTypes.Take(2).ToArray();
            Type sourceType;
            return
                argTypes.Length == 1 && argTypes[0] == typeof(int) &&
                container is INotifyCollectionChanged &&
                (sourceType = container.GetType()).HasInterface(typeof(IEnumerable<>).MakeGenericType(sourceType.GetItemType(argTypes)));
        }

        public IObservable<ObservedChange> GetChanges(object container, DataMemberAccessLink link)
        {
            var indexerLink = (IndexerAccessLink)link;
            var ncc = (INotifyCollectionChanged)container;
            var index = (int)indexerLink.GetArgument(0);

            return Observable.FromEventPattern<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
                handler => ncc.CollectionChanged += handler, handler => ncc.CollectionChanged -= handler)
                .Where(e =>
                {
                    NotifyCollectionChangedEventArgs args = e.EventArgs;
                    switch (args.Action)
                    {
                        case NotifyCollectionChangedAction.Reset:
                            return true;
                        case NotifyCollectionChangedAction.Add:
                            return IsAffected(index, args.NewStartingIndex, args.NewItems);
                        case NotifyCollectionChangedAction.Remove:
                            return IsAffected(index, args.OldStartingIndex, args.OldItems);
                        case NotifyCollectionChangedAction.Move:
                        case NotifyCollectionChangedAction.Replace:
                            return IsAffected(index, args.OldStartingIndex, args.OldItems) || IsAffected(index, args.NewStartingIndex, args.NewItems);
                        default:
                            throw new NotSupportedException();
                    }
                })
                .Select(_ => new ObservedChange(ncc, indexerLink));

            bool IsAffected(int idx, int startIdx, IList items)
            {
                var count = items?.Count ?? 0;
                return startIdx >= 0 && startIdx <= idx && idx < startIdx + count;
            }
        }
    }
}
