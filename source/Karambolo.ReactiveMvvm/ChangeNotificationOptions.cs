using System;

namespace Karambolo.ReactiveMvvm
{
    [Flags]
    public enum ChangeNotificationOptions
    {
        None = 0,
        BeforeChange = 0x1,
        SkipInitial = 0x2,
        NonDistinct = 0x4,
        Default = None
    }
}
