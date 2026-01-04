using System;
using System.Collections.Generic;
using System.ComponentModel;
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
#pragma warning disable IL2072 // valid but we can't do anything about it without sourcegen (AsPreserved can be used as a workaround)
            return container is DependencyObject && DependencyObjectHelper.GetDependencyPropertyDescriptorCached(container.GetType(), ((FieldOrPropertyAccessLink)link).MemberName) != null;
#pragma warning restore IL2072
        }

        public IObservable<ObservedChange> GetChanges(object container, DataMemberAccessLink link)
        {
#pragma warning disable IL2072 // see CanProvideFor
            DependencyPropertyDescriptor dpd = DependencyObjectHelper.GetDependencyPropertyDescriptorCached(container.GetType(), ((FieldOrPropertyAccessLink)link).MemberName);
#pragma warning restore IL2072

            return Observable.Create<ObservedChange>(observer =>
            {
                EventHandler handler = (s, e) => observer.OnNext(new ObservedChange(container, link));
                dpd.AddValueChanged(container, handler);
                return Disposable.Create(() => dpd.RemoveValueChanged(container, handler));
            });
        }
    }
}
