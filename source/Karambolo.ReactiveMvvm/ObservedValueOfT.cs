using System;
using System.Collections.Generic;

namespace Karambolo.ReactiveMvvm
{
    public readonly struct ObservedValue<T> : IEquatable<ObservedValue<T>>
    {
        public static implicit operator ObservedValue<T>(ObservedValue _)
        {
            return default;
        }

        public static implicit operator ObservedValue<T>(in T value)
        {
            return new ObservedValue<T>(value);
        }

        public ObservedValue(in T value)
        {
            Value = value;
            IsAvailable = true;
        }

        public T Value { get; }
        public bool IsAvailable { get; }

        public T GetValueOrDefault()
        {
            return GetValueOrDefault(default);
        }

        public T GetValueOrDefault(T defaultValue)
        {
            return IsAvailable ? Value : defaultValue;
        }

        public ObservedValue<TTarget> Cast<TTarget>()
        {
            return IsAvailable ? ObservedValue.From((TTarget)(object)Value) : ObservedValue.None;
        }

        public bool Equals(ObservedValue<T> other)
        {
            return IsAvailable == other.IsAvailable &&
                (!IsAvailable || EqualityComparer<T>.Default.Equals(Value, other.Value));
        }

        public override bool Equals(object obj)
        {
            return obj is ObservedValue<T> other ? Equals(other) : false;
        }

        public override int GetHashCode()
        {
            return !IsAvailable ? -1 : Value == null ? 0 : Value.GetHashCode();
        }

        public override string ToString()
        {
            return !IsAvailable ? "(n/a)" : Value == null ? "(null)" : Value.ToString();
        }
    }
}
