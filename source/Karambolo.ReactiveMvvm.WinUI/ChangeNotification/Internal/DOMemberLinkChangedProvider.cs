#if TARGETS_WINUI || IS_UNO

using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Karambolo.ReactiveMvvm.Expressions;
using Karambolo.ReactiveMvvm.Helpers;
using Microsoft.UI.Xaml;

namespace Karambolo.ReactiveMvvm.ChangeNotification.Internal
{
    public class DOMemberLinkChangedProvider : ILinkChangeProvider
    {
        public bool NotifiesBeforeChange => false;

        public IEnumerable<Type> SupportedLinkTypes => new[] { typeof(FieldOrPropertyAccessLink) };

        public bool CanProvideFor(object container, DataMemberAccessLink link)
        {
#pragma warning disable IL2072 // valid but we can't do anything about it without sourcegen (AsPreserved can be used as a workaround)
            return container is DependencyObject && DependencyObjectHelper.GetDependencyPropertyCached(container.GetType(), ((FieldOrPropertyAccessLink)link).MemberName) != null;
#pragma warning restore IL2072
        }

        public IObservable<ObservedChange> GetChanges(object container, DataMemberAccessLink link)
        {
            var @do = (DependencyObject)container;
#pragma warning disable IL2072 // see CanProvideFor
            DependencyProperty dp = DependencyObjectHelper.GetDependencyPropertyCached(container.GetType(), ((FieldOrPropertyAccessLink)link).MemberName);
#pragma warning restore IL2072

            return Observable.Create<ObservedChange>(observer =>
            {
                var token = @do.RegisterPropertyChangedCallback(dp, delegate
                {
                    observer.OnNext(new ObservedChange(@do, link));
                });
                return Disposable.Create(() => @do.UnregisterPropertyChangedCallback(dp, token));
            });
        }
    }
}

#endif
