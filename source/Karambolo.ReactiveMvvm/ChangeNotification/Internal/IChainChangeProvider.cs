using System;
using Karambolo.ReactiveMvvm.Expressions;

namespace Karambolo.ReactiveMvvm.ChangeNotification.Internal
{
    public interface IChainChangeProvider
    {
        IObservable<ObservedValue<object>> GetChanges(object root, DataMemberAccessChain chain, bool beforeChange);
    }
}
