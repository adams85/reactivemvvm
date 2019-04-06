using System;

namespace Karambolo.ReactiveMvvm.Binding
{
    [Flags]
    internal enum DataBindingEventFlags
    {
        None = 0,
        FlowsToSource = 0x1,
        ConversionFailed = 0x2,
    }
}
