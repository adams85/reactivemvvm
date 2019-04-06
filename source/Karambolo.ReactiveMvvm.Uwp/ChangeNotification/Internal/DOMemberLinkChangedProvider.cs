using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Karambolo.Common;
using Karambolo.ReactiveMvvm.Expressions;
using Karambolo.ReactiveMvvm.Helpers;
using Windows.UI.Xaml;

namespace Karambolo.ReactiveMvvm.ChangeNotification.Internal
{
    public class DOMemberLinkChangedProvider : ILinkChangeProvider
    {
        public bool NotifiesBeforeChange => false;

        public IEnumerable<Type> SupportedLinkTypes => EnumerableUtils.Return(typeof(FieldOrPropertyAccessLink));

        public bool CanProvideFor(object container, DataMemberAccessLink link)
        {
            return container is DependencyObject && DependencyObjectHelper.GetDependencyPropertyCached(container.GetType(), ((FieldOrPropertyAccessLink)link).Member.Name) != null;
        }

        public IObservable<ObservedChange> GetChanges(object container, DataMemberAccessLink link)
        {
            var @do = (DependencyObject)container;
            DependencyProperty dp = DependencyObjectHelper.GetDependencyPropertyCached(container.GetType(), ((FieldOrPropertyAccessLink)link).Member.Name);

            return Observable.Create<ObservedChange>(observer =>
            {
                var token = @do.RegisterPropertyChangedCallback(dp, (o, p) => observer.OnNext(new ObservedChange(container, link)));
                return Disposable.Create(() => @do.UnregisterPropertyChangedCallback(dp, token));
            });
        }
    }
}
