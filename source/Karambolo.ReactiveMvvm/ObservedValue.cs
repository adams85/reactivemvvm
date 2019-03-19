namespace Karambolo.ReactiveMvvm
{
    public readonly struct ObservedValue
    {
        public static readonly ObservedValue None = default;

        public static ObservedValue<T> From<T>(in T value)
        {
            return value;
        }

        public static ObservedValue<T> Wrap<T>(in T value)
        {
            return value != null ? From(value) : None;
        }
    }
}
