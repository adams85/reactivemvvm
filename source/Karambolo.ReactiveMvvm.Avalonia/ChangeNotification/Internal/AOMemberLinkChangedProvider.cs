using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using Avalonia;
using Karambolo.ReactiveMvvm.Expressions;
using Karambolo.ReactiveMvvm.Helpers;

namespace Karambolo.ReactiveMvvm.ChangeNotification.Internal
{
    public class AOMemberLinkChangedProvider : ILinkChangeProvider
    {
        public bool NotifiesBeforeChange => false;

        public IEnumerable<Type> SupportedLinkTypes => new[] { typeof(FieldOrPropertyAccessLink) };

        public bool CanProvideFor(object container, DataMemberAccessLink link)
        {
            return container is AvaloniaObject && AvaloniaObjectHelper.GetAvaloniaPropertyCached(container.GetType(), ((FieldOrPropertyAccessLink)link).Member.Name) != null;
        }

        public IObservable<ObservedChange> GetChanges(object container, DataMemberAccessLink link)
        {
            AvaloniaProperty ap = AvaloniaObjectHelper.GetAvaloniaPropertyCached(container.GetType(), ((FieldOrPropertyAccessLink)link).Member.Name);

            return ap.Changed
                .Where(change => change.Sender == container)
                .Select(_ => new ObservedChange(container, link));
        }
    }
}
