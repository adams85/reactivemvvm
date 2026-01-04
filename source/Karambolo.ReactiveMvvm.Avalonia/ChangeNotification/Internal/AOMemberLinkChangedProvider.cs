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
#pragma warning disable IL2072 // valid but we can't do anything about it without sourcegen (AsPreserved can be used as a workaround)
            return container is AvaloniaObject && AvaloniaObjectHelper.GetAvaloniaPropertyCached(container.GetType(), ((FieldOrPropertyAccessLink)link).MemberName) != null;
#pragma warning restore IL2072
        }

        public IObservable<ObservedChange> GetChanges(object container, DataMemberAccessLink link)
        {
#pragma warning disable IL2072 // see CanProvideFor
            AvaloniaProperty ap = AvaloniaObjectHelper.GetAvaloniaPropertyCached(container.GetType(), ((FieldOrPropertyAccessLink)link).MemberName);
#pragma warning restore IL2072

            return ap.Changed
                .Where(change => change.Sender == container)
                .Select(_ => new ObservedChange(container, link));
        }
    }
}
