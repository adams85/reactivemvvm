using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using Karambolo.ReactiveMvvm.Expressions;

namespace Karambolo.ReactiveMvvm.ChangeNotification.Internal
{
    public class NpcMemberLinkChangingProvider : ILinkChangeProvider
    {
        public bool NotifiesBeforeChange => true;

        public IEnumerable<Type> SupportedLinkTypes => new[] { typeof(FieldOrPropertyAccessLink) };

        public bool CanProvideFor(object container, DataMemberAccessLink link)
        {
            return container is INotifyPropertyChanging;
        }

        public IObservable<ObservedChange> GetChanges(object container, DataMemberAccessLink link)
        {
            var memberLink = (FieldOrPropertyAccessLink)link;
            var npc = (INotifyPropertyChanging)container;

            return Observable.FromEventPattern<PropertyChangingEventHandler, PropertyChangingEventArgs>(
                handler => npc.PropertyChanging += handler, handler => npc.PropertyChanging -= handler)
                .Where(e => string.IsNullOrEmpty(e.EventArgs.PropertyName) || e.EventArgs.PropertyName == memberLink.MemberName)
                .Select(_ => new ObservedChange(npc, memberLink));
        }
    }
}
