using System;

namespace Karambolo.ReactiveMvvm
{
    public static class DisposableExtensions
    {
        public static T AttachTo<T>(this T disposable, ILifetime lifetime)
            where T : IDisposable
        {
            if (disposable == null)
                throw new ArgumentNullException(nameof(disposable));

            if (lifetime == null)
                throw new ArgumentNullException(nameof(lifetime));

            lifetime.AttachDisposable(disposable);

            return disposable;
        }

        public static T DetachFrom<T>(this T disposable, ILifetime lifetime)
            where T : IDisposable
        {
            if (disposable == null)
                throw new ArgumentNullException(nameof(disposable));

            if (lifetime == null)
                throw new ArgumentNullException(nameof(lifetime));

            lifetime.DetachDisposable(disposable);

            return disposable;
        }
    }
}
