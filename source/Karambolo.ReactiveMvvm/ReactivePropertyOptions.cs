using System;

namespace Karambolo.ReactiveMvvm
{
    [Flags]
    public enum ReactivePropertyOptions
    {
        None = 0,
        DeferSubscription = 0x1,
        SkipInitial = 0x2,
        Default = None
    }
}
