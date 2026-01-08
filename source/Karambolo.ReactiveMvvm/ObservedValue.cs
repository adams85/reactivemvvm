using System.Runtime.CompilerServices;

namespace Karambolo.ReactiveMvvm
{
    public readonly struct ObservedValue
    {
        public static readonly ObservedValue None = default;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ObservedValue<T> From<T>(T value)
        {
            return new ObservedValue<T>(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ObservedValue<T> Wrap<T>(T value)
        {
            return value != null ? new ObservedValue<T>(value) : None;
        }
    }
}
