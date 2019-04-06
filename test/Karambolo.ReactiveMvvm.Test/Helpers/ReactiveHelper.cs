using System;
using System.Reactive;

namespace Karambolo.ReactiveMvvm.Test.Helpers
{
    internal static class ReactiveHelper
    {
        public static IObserver<T> CreateObserver<T>(this Sequence<T> output)
        {
            return Observer.Create<T>(
                value => output.Add(Notification.CreateOnNext(value)),
                ex => output.Add(Notification.CreateOnError<T>(ex)),
                () => output.Add(Notification.CreateOnCompleted<T>()));
        }
    }
}
