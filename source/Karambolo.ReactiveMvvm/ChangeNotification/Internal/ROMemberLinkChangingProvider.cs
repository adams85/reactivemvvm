using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Karambolo.ReactiveMvvm.Expressions;

namespace Karambolo.ReactiveMvvm.ChangeNotification.Internal
{
    public class ROMemberLinkChangingProvider : ILinkChangeProvider
    {
        public bool NotifiesBeforeChange => true;

        public IEnumerable<Type> SupportedLinkTypes => new[] { typeof(FieldOrPropertyAccessLink) };

        public bool CanProvideFor(object container, DataMemberAccessLink link)
        {
            return container is ReactiveObject;
        }

        public IObservable<ObservedChange> GetChanges(object container, DataMemberAccessLink link)
        {
            var memberLink = (FieldOrPropertyAccessLink)link;
            var obj = (ReactiveObject)container;

            return obj.WhenChanging
                .Where(e => string.IsNullOrEmpty(e.EventArgs.PropertyName) || e.EventArgs.PropertyName == memberLink.Member.Name)
                .Select(_ => new ObservedChange(obj, memberLink));
        }
    }
}
