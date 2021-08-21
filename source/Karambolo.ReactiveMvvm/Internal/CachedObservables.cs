using System;
using System.Reactive;

namespace Karambolo.ReactiveMvvm.Internal
{
    public static class CachedObservables
    {
        public static class Empty<T>
        {
            public static readonly IObservable<T> Observable = System.Reactive.Linq.Observable.Empty<T>();
        }

        public static class Never<T>
        {
            public static readonly IObservable<T> Observable = System.Reactive.Linq.Observable.Never<T>();
        }

        public static class Default<T>
        {
            public static readonly IObservable<T> Observable = System.Reactive.Linq.Observable.Return(default(T));
        }

        public static class False
        {
            public static readonly IObservable<bool> Observable = System.Reactive.Linq.Observable.Return(false);
        }

        public static class True
        {
            public static readonly IObservable<bool> Observable = System.Reactive.Linq.Observable.Return(true);
        }

        public static class Unit
        {
            public static readonly IObservable<System.Reactive.Unit> Observable = System.Reactive.Linq.Observable.Return(System.Reactive.Unit.Default);
        }
    }
}
