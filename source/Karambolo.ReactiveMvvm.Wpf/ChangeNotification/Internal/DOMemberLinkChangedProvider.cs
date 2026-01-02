using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using Karambolo.ReactiveMvvm.Expressions;
using Karambolo.ReactiveMvvm.Helpers;

namespace Karambolo.ReactiveMvvm.ChangeNotification.Internal
{
    public class DOMemberLinkChangedProvider : ILinkChangeProvider
    {
        public bool NotifiesBeforeChange => false;

        public IEnumerable<Type> SupportedLinkTypes => new[] { typeof(FieldOrPropertyAccessLink) };

        public bool CanProvideFor(object container, DataMemberAccessLink link)
        {
            return container is DependencyObject && DependencyObjectHelper.GetDependencyPropertyDescriptorCached(container.GetType(), ((FieldOrPropertyAccessLink)link).Member.Name) != null;
        }

        public IObservable<ObservedChange> GetChanges(object container, DataMemberAccessLink link)
        {
            System.ComponentModel.DependencyPropertyDescriptor dpd = DependencyObjectHelper.GetDependencyPropertyDescriptorCached(container.GetType(), ((FieldOrPropertyAccessLink)link).Member.Name);

            return Observable.Create<ObservedChange>(observer =>
            {
                EventHandler handler = (s, e) => observer.OnNext(new ObservedChange(container, link));
                dpd.AddValueChanged(container, handler);
                return Disposable.Create(() => dpd.RemoveValueChanged(container, handler));
            });
        }
    }
}
