using System;
using System.Diagnostics.CodeAnalysis;
using System.Reactive.Concurrency;
using System.Reflection;
using System.Windows.Input;

namespace Karambolo.ReactiveMvvm.Binding.Internal
{
    public interface ICommandBinder
    {
        bool CanBind(
#if NET5_0_OR_GREATER
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicEvents)]
#endif
            Type containerType,
            string eventName);

        IDisposable Bind<
#if NET5_0_OR_GREATER
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicEvents)]
#endif
            TContainer,
            TParam>(
                ICommand command, TContainer container, IObservable<TParam> commandParameters, string eventName,
                IScheduler scheduler, Action<Exception> onError);

        MemberInfo GetContainerMember(
#if NET5_0_OR_GREATER
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicEvents)]
#endif
            Type containerType,
            string eventName);
    }
}
