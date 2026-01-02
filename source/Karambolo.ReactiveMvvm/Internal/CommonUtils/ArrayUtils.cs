using System;
using System.Runtime.CompilerServices;

namespace Karambolo.Common
{
    internal static class ArrayUtils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrEmpty(this Array array)
        {
            return
                array == null ||
                array.LongLength == 0;
        }
    }
}
