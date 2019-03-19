using System;
using System.Collections.Generic;
using Karambolo.ReactiveMvvm.Expressions;

namespace Karambolo.ReactiveMvvm.ChangeNotification.Internal
{
    public interface ILinkChangeProvider
    {
        IEnumerable<Type> SupportedLinkTypes { get; }
        bool NotifiesBeforeChange { get; }
        bool CanProvideFor(object container, DataMemberAccessLink link);
        IObservable<ObservedChange> GetChanges(object container, DataMemberAccessLink link);
    }
}
