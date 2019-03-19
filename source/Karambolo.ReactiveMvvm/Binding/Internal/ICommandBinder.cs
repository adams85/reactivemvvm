using System;
using System.Reactive.Concurrency;
using System.Reflection;
using System.Windows.Input;

namespace Karambolo.ReactiveMvvm.Binding.Internal
{
    public interface ICommandBinder
    {
        bool CanBind(Type containerType, string eventName);
        IDisposable Bind<TParam>(ICommand command, object container, IObservable<TParam> commandParameters, string eventName, IScheduler scheduler, Action<Exception> onError);
        MemberInfo GetContainerMember(Type containerType, string eventName);
    }
}
