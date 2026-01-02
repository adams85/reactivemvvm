using System.Runtime.CompilerServices;

namespace Karambolo.Common
{
    internal static class GeneralUtils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Swap<T>(ref T value1, ref T value2)
        {
            T temp = value1;
            value1 = value2;
            value2 = temp;
        }
    }
}
