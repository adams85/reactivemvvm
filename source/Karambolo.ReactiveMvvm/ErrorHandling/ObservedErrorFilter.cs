using System;

namespace Karambolo.ReactiveMvvm.ErrorHandling
{
    public delegate IObservable<T> ObservedErrorFilter<out T>(ObservedErrorException exception);
}
