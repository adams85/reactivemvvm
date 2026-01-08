using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Karambolo.ReactiveMvvm
{
    public readonly struct ObservedValue<T> : IEquatable<ObservedValue<T>>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ObservedValue<T>(ObservedValue _)
        {
            return default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ObservedValue<T>(T value)
        {
            return new ObservedValue<T>(value);
        }

        public ObservedValue(T value)
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
            return
                obj is ObservedValue<T> other ?
                Equals(other) :
                obj is ObservedValue && Equals(default);
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
